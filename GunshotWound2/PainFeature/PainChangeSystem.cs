﻿namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using HealthFeature;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using States;

    public sealed class PainChangeSystem : ISystem {
        private readonly SharedData sharedData;

        private readonly IPainState[] painStates = {
            new MildPainState(),
            new AveragePainState(),
            new IntensePainState(),
            new UnbearablePainState(),
            new DeadlyPainState(),
        };

        private Filter pedsWithPain;
        private Stash<ConvertedPed> pedStash;
        private Stash<Pain> painStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;

        public World World { get; set; }

        public PainChangeSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            pedsWithPain = World.Filter.With<ConvertedPed>().With<Pain>();
            pedStash = World.GetStash<ConvertedPed>();
            painStash = World.GetStash<Pain>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();

            Array.Sort(painStates, (x, y) => x.PainThreshold.CompareTo(y.PainThreshold));
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in pedsWithPain) {
                if (totallyHealedStash.Has(entity)) {
                    ResetPain(entity);
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref Pain pain = ref painStash.Get(entity);
                if (pain.diff > 0f) {
                    ApplyPain(ref convertedPed, ref pain);
                } else if (pain.HasPain()) {
                    pain.amount -= pain.recoveryRate * deltaTime;
                } else {
                    continue;
                }

                if (entity.Has<JustConvertedEvent>()) {
                    pain.currentState = null;
                }

                RefreshPainState(entity, ref convertedPed, ref pain);
            }
        }

        public void Dispose() {
            foreach (Entity entity in pedsWithPain) {
                ResetPain(entity);
            }
        }

        private void ApplyPain(ref ConvertedPed convertedPed, ref Pain pain) {
            PlayPainEffects(ref convertedPed, ref pain);

            float diff = pain.diff;
            pain.amount += diff;
            pain.diff = 0f;
        }

        private void RefreshPainState(Entity entity, ref ConvertedPed convertedPed, ref Pain pain) {
            float painPercent = pain.Percent();

            int newStateIndex = -1;
            int curStateIndex = -1;
            for (var i = 0; i < painStates.Length; i++) {
                IPainState painState = painStates[i];
                if (pain.currentState == painState) {
                    curStateIndex = i;
                }

                if (painPercent >= painState.PainThreshold) {
                    newStateIndex = i;
                }
            }

            if (curStateIndex != newStateIndex) {
#if DEBUG
                string currentName = curStateIndex >= 0 ? painStates[curStateIndex].GetType().Name : "NO PAIN";
                string newName = newStateIndex >= 0 ? painStates[newStateIndex].GetType().Name : "NO PAIN";
                sharedData.logger.WriteInfo($"Changed pain state of {convertedPed.name}: {currentName} => {newName}");
#endif

                //TODO Localize
                int direction = Math.Sign(newStateIndex - curStateIndex);
                if (convertedPed.isPlayer) {
                    sharedData.notifier.info.AddMessage(direction > 0 ? "~o~You feel yourself worst" : "~g~You feel yourself better");
                }

                IPainState curState = null;
                while (curStateIndex != newStateIndex) {
                    IPainState prevState = curStateIndex >= 0 ? painStates[curStateIndex] : null;
                    curStateIndex += direction;
                    curState = curStateIndex >= 0 ? painStates[curStateIndex] : null;

                    if (direction > 0) {
                        curState?.ApplyPainIncreased(sharedData, entity, ref convertedPed);
                    } else {
                        prevState?.ApplyPainDecreased(sharedData, entity, ref convertedPed);
                    }
                }

                pain.currentState = curState;
                RefreshMoveSet(ref convertedPed, curState);
            }

            RefreshMoveRate(ref convertedPed, ref pain);
        }

        private void RefreshMoveSet(ref ConvertedPed convertedPed, IPainState state) {
            if (state != null && state.TryGetMoveSets(sharedData, convertedPed.isPlayer, out string[] moveSets)) {
                PedEffects.ChangeMoveSetRandom(convertedPed.thisPed, moveSets, sharedData.random);
            } else {
                PedEffects.ResetMoveSet(convertedPed.thisPed);
            }
        }

        private void RefreshMoveRate(ref ConvertedPed convertedPed, ref Pain pain) {
            // if (woundedPed.Crits.Has(CritTypes.LEGS_DAMAGED))
            //     continue;

            if (!pain.HasPain() || convertedPed.hasBrokenLegs) {
                return;
            }

            float painPercent = pain.Percent();
            if (painPercent >= 1f) {
                PedEffects.OverrideMoveRate(convertedPed.thisPed, sharedData.mainConfig.WoundConfig.MoveRateOnFullPain);
                return;
            }

            float adjustable = 1f - sharedData.mainConfig.WoundConfig.MoveRateOnFullPain;
            float moveRate = 1f - adjustable * painPercent;
            PedEffects.OverrideMoveRate(convertedPed.thisPed, moveRate);
        }

        private void ResetPain(Entity entity) {
            ref Pain pain = ref painStash.Get(entity);
            pain.amount = 0f;
            pain.diff = 0f;

            RefreshPainState(entity, ref pedStash.Get(entity), ref pain);
        }

        private void PlayPainEffects(ref ConvertedPed convertedPed, ref Pain pain) {
            if (convertedPed.hasSpineDamage) {
                return;
            }
            
            int painAnimIndex = sharedData.random.Next(1, 7);
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"pain_{painAnimIndex.ToString()}", convertedPed.isMale);

            WoundConfig woundConfig = sharedData.mainConfig.WoundConfig;
            float painfulThreshold = woundConfig.PainfulWoundPercent * pain.max;
            if (pain.diff >= painfulThreshold) {
#if DEBUG
                sharedData.logger.WriteInfo($"Painful wound {pain.diff.ToString("F")} at {convertedPed.name}");
#endif

                convertedPed.thisPed.PlayAmbientSpeech("GUN_BEG");
                if (woundConfig.RagdollOnPainfulWound) {
                    convertedPed.RequestRagdoll(timeInMs: 3000, GTA.RagdollType.Balance);
                }

                if (convertedPed.isPlayer) {
                    CameraEffects.ShakeCameraOnce();
                    CameraEffects.FlashCameraOnce();
                }
            }
        }
    }
}