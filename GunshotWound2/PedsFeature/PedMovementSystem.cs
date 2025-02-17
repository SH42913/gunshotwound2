namespace GunshotWound2.PedsFeature {
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class PedMovementSystem : ILateSystem {
        private const float DEFAULT_MOVE_RATE = 1f;

        private readonly SharedData sharedData;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;

        public World World { get; set; }

        public PedMovementSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (convertedPed.thisPed.IsInVehicle()) {
                    VehicleMovement(ref convertedPed);
                } else {
                    NonVehicleMovement(ref convertedPed);
                }
            }
        }

        private void VehicleMovement(ref ConvertedPed convertedPed) {
            if (!convertedPed.isRestrictToDrive && !convertedPed.hasSpineDamage) {
                return;
            }

            GTA.Vehicle vehicle = convertedPed.thisPed.CurrentVehicle;
            if (vehicle.Driver != convertedPed.thisPed) {
                return;
            }

            if (convertedPed.isPlayer) {
                PlayerEffects.DisableVehicleControlThisFrame();
            } else {
                PedEffects.SetVehicleOutOfControl(vehicle);
            }
        }

        private void NonVehicleMovement(ref ConvertedPed convertedPed) {
            if (convertedPed.isRagdoll) {
                return;
            }

            if (convertedPed.moveRate > 0f) {
                PedEffects.OverrideMoveRate(convertedPed.thisPed, convertedPed.moveRate);
            }

            if (convertedPed.resetMoveSet) {
                PedEffects.ResetMoveSet(convertedPed.thisPed);
                convertedPed.moveSetRequest = null;
                convertedPed.resetMoveSet = false;
                convertedPed.hasCustomMoveSet = false;
            } else if (PedEffects.TryRequestMoveSet(convertedPed.moveSetRequest)) {
                PedEffects.ChangeMoveSet(convertedPed.thisPed, convertedPed.moveSetRequest);
                convertedPed.moveSetRequest = null;
                convertedPed.hasCustomMoveSet = true;
            }

            if (convertedPed.sprintBlockers > 0) {
                convertedPed.thisPed.SetConfigFlag(GTA.PedConfigFlagToggles.IsInjured, true);
                if (convertedPed.isPlayer) {
                    PlayerEffects.SetSprint(false);
                }
            } else if (convertedPed.isPlayer) {
                PlayerEffects.SetSprint(true);
            }
        }

        public void Dispose() {
            foreach (Entity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                PedEffects.OverrideMoveRate(convertedPed.thisPed, DEFAULT_MOVE_RATE);
                if (convertedPed.hasCustomMoveSet) {
                    PedEffects.ResetMoveSet(convertedPed.thisPed);
                }
            }
        }
    }
}