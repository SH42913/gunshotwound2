namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using States;
    using Utils;

    public sealed class PainChangeSystem : ILateSystem {
        private static readonly int[] NM_MESSAGES = { 548, };

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

                if (convertedPed.isPlayer && entity.Has<JustConvertedEvent>()) {
                    sharedData.cameraService.ClearAllEffects();
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

#if DEBUG
            sharedData.logger.WriteInfo($"Increased pain for {diff.ToString("F5")} at {convertedPed.name}");
#endif
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

                int direction = Math.Sign(newStateIndex - curStateIndex);
                if (convertedPed.isPlayer && direction < 0) {
                    sharedData.notifier.info.QueueMessage(sharedData.localeConfig.PainDecreasedMessage, Notifier.Color.GREEN);
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
                RefreshMood(ref convertedPed, curState);
            }

            RefreshMoveRate(ref convertedPed, ref pain);
        }

        private void RefreshMoveSet(ref ConvertedPed convertedPed, IPainState state) {
            if (state != null && state.TryGetMoveSets(sharedData.mainConfig, convertedPed, out string[] moveSets)) {
                convertedPed.moveSetRequest = moveSets != null && moveSets.Length > 0
                        ? sharedData.random.Next(moveSets)
                        : null;
            } else {
                convertedPed.resetMoveSet = true;
            }
        }

        private void RefreshMood(ref ConvertedPed convertedPed, IPainState state) {
            if (state != null && state.TryGetMoodSets(sharedData.mainConfig, convertedPed, out string[] sets) && sets.Length > 0) {
                PedEffects.SetFacialIdleAnim(convertedPed.thisPed, sharedData.random.Next(sets), convertedPed.isMale);
            } else {
                PedEffects.CleanFacialIdleAnim(convertedPed.thisPed);
            }
        }

        private void RefreshMoveRate(ref ConvertedPed convertedPed, ref Pain pain) {
            if (convertedPed.hasBrokenLegs) {
                return;
            }

            if (!pain.HasPain()) {
                convertedPed.ResetMoveRate();
                return;
            }

            float painPercent = pain.Percent();
            if (painPercent >= 1f) {
                convertedPed.moveRate = sharedData.mainConfig.WoundConfig.MoveRateOnFullPain;
                return;
            }

            float adjustable = 1f - sharedData.mainConfig.WoundConfig.MoveRateOnFullPain;
            float moveRate = 1f - adjustable * painPercent;
            convertedPed.moveRate = moveRate;
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

                convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS");
                if (woundConfig.RagdollOnPainfulWound) {
                    convertedPed.RequestRagdoll(timeInMs: 1500);
                }

                if (convertedPed.isPlayer) {
                    sharedData.cameraService.PlayPainfulWoundEffect();
                } else if (convertedPed.thisPed.IsOnBike) {
                    convertedPed.thisPed.Task.LeaveVehicle(GTA.LeaveVehicleFlags.BailOut);
                }
            }
        }
    }
}