namespace GunshotWound2.PainFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;
    using States;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PainStateSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly IPainState[] painStates;

        private Filter pedsWithPain;
        private Stash<ConvertedPed> pedStash;
        private Stash<Pain> painStash;

        public EcsWorld World { get; set; }

        public PainStateSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            painStates = new IPainState[] {
                new MildPainState(sharedData),
                new AveragePainState(sharedData),
                new IntensePainState(sharedData),
                new UnbearablePainState(sharedData),
                new DeadlyPainState(sharedData),
            };
        }

        public void OnAwake() {
            pedsWithPain = World.Filter.With<ConvertedPed>().With<Pain>();
            pedStash = World.GetStash<ConvertedPed>();
            painStash = World.GetStash<Pain>();

            Array.Sort(painStates, (x, y) => x.PainThreshold.CompareTo(y.PainThreshold));
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in pedsWithPain) {
                ref Pain pain = ref painStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (convertedPed.isPlayer && entity.Has<JustConvertedEvent>()) {
                    sharedData.cameraService.ClearAllEffects();
                    pain.currentState = null;
                }

                GetCurAndNewStateIndex(pain, out int curStateIndex, out int newStateIndex);
                ChangePainState(entity, ref convertedPed, ref pain, curStateIndex, newStateIndex);
            }
        }

        public void Dispose() {
            foreach (EcsEntity entity in pedsWithPain) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref Pain pain = ref painStash.Get(entity);
                GetCurAndNewStateIndex(pain, out int curStateIndex, out int _);
                ChangePainState(entity, ref convertedPed, ref pain, curStateIndex, newStateIndex: -1);
            }
        }

        private void ChangePainState(EcsEntity entity,
                                     ref ConvertedPed convertedPed,
                                     ref Pain pain,
                                     int curStateIndex,
                                     int newStateIndex) {
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
                        curState?.ApplyPainIncreased(entity, ref convertedPed);
                    } else {
                        prevState?.ApplyPainDecreased(entity, ref convertedPed);
                    }
                }

                pain.currentState = curState;
                RefreshMoveSet(ref convertedPed, curState);
                RefreshMood(ref convertedPed, curState);
            }

            RefreshMoveRate(ref convertedPed, ref pain);
        }

        private void GetCurAndNewStateIndex(in Pain pain, out int curStateIndex, out int newStateIndex) {
            float painPercent = pain.Percent();

            curStateIndex = -1;
            newStateIndex = -1;
            for (var i = 0; i < painStates.Length; i++) {
                IPainState painState = painStates[i];
                if (pain.currentState == painState) {
                    curStateIndex = i;
                }

                if (painPercent >= painState.PainThreshold) {
                    newStateIndex = i;
                }
            }
        }

        private void RefreshMoveSet(ref ConvertedPed convertedPed, IPainState state) {
            if (state != null && state.TryGetMoveSets(convertedPed, out string[] moveSets)) {
                convertedPed.moveSetRequest = moveSets != null && moveSets.Length > 0
                        ? sharedData.random.Next(moveSets)
                        : null;
            } else {
                convertedPed.resetMoveSet = true;
            }
        }

        private void RefreshMood(ref ConvertedPed convertedPed, IPainState state) {
            if (state != null && state.TryGetMoodSets(convertedPed, out string[] sets) && sets.Length > 0) {
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
                convertedPed.moveRate = sharedData.mainConfig.woundConfig.MoveRateOnFullPain;
                return;
            }

            float adjustable = 1f - sharedData.mainConfig.woundConfig.MoveRateOnFullPain;
            float moveRate = 1f - adjustable * painPercent;
            convertedPed.moveRate = moveRate;
        }
    }
}