namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using HealthCare;
    using Peds;
    using Scellecs.Morpeh;
    using States;

    public sealed class PainChangeSystem : ISystem {
        private readonly SharedData sharedData;

        private readonly IPainState noPainState = new NoPainState();
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
                    return;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref Pain pain = ref painStash.Get(entity);
                if (pain.diff > 0f) {
                    ApplyPain(ref convertedPed, ref pain);
                } else if (pain.amount > 0f) {
                    pain.amount -= pain.recoveryRate * deltaTime;
                }

                RefreshPainState(entity, ref convertedPed, ref pain);
                RefreshMoveRate(ref convertedPed, ref pain);
            }
        }

        public void Dispose() {
            foreach (Entity entity in pedsWithPain) {
                ResetPain(entity);
            }
        }

        private void ApplyPain(ref ConvertedPed convertedPed, ref Pain pain) {
            float diff = pain.diff;
            pain.amount += diff;
            pain.diff = 0f;

            // var painAnimIndex = GunshotWound2.Random.Next(1, 6);
            // PainChangeSystem.PlayFacialAnim(woundedPed, $"pain_{painAnimIndex.ToString()}");

            WoundConfig woundConfig = sharedData.mainConfig.WoundConfig;
            float painfulThreshold = woundConfig.PainfulWoundPercent * pain.max;
            if (diff < painfulThreshold) {
                return;
            }

            if (woundConfig.RagdollOnPainfulWound) {
                // _ecsWorld.CreateEntityWith(out SetPedToRagdollEvent ragdoll);
                // ragdoll.Entity = pedEntity;
                // ragdoll.RagdollState = RagdollStates.SHORT;
            }

            if (convertedPed.isPlayer) {
                // _ecsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.ONE_TIME;
                // _ecsWorld.CreateEntityWith<AddFlashEvent>();
            }
        }

        private void RefreshPainState(Entity entity, ref ConvertedPed convertedPed, ref Pain pain) {
            float painPercent = pain.amount / pain.max;
            IPainState newState = null;

            foreach (IPainState painState in painStates) {
                if (painPercent >= painState.PainThreshold) {
                    newState = painState;
                }
            }

            if (newState == pain.currentState) {
                return;
            }
#if DEBUG
            string currentName = pain.currentState?.GetType().Name ?? "NO PAIN";
            string newName = newState?.GetType().Name ?? "NO PAIN";
            sharedData.logger.WriteInfo($"Changed pain state: {currentName} => {newName}");
#endif
            pain.currentState = newState;

            if (pain.currentState != null) {
                pain.currentState.ApplyState(entity, ref convertedPed);
            } else {
                noPainState.ApplyState(entity, ref convertedPed);
            }
        }

        private void RefreshMoveRate(ref ConvertedPed convertedPed, ref Pain pain) {
            // if (woundedPed.Crits.Has(CritTypes.LEGS_DAMAGED))
            //     continue;

            // float backPercent = painPercent > 1
            //         ? 0
            //         : 1 - painPercent;
            // float adjustable = 1f - _config.Data.WoundConfig.MoveRateOnFullPain;
            // float moveRate = 1f - adjustable * backPercent;
            // Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, moveRate);
        }

        private void ResetPain(Entity entity) {
            ref Pain pain = ref painStash.Get(entity);
            pain.amount = 0f;
            pain.diff = 0f;

            noPainState.ApplyState(entity, ref pedStash.Get(entity));
        }

        // protected void ChangeMoveSet(int pedEntity, string[] moveSets) {
        //     EcsWorld.CreateEntityWith(out SwitchMoveSetRequest request);
        //     request.Entity = pedEntity;
        //     request.AnimationName = moveSets != null && moveSets.Length > 0
        //             ? moveSets[GunshotWound2.Random.Next(0, moveSets.Length)]
        //             : null;
        // }
    }
}