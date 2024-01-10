namespace GunshotWound2.PedsFeature {
    using Scellecs.Morpeh;

    public sealed class PedMovementSystem : ISystem {
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
                if (convertedPed.isRagdoll) {
                    continue;
                }

                if (convertedPed.moveRate > 0f) {
                    PedEffects.OverrideMoveRate(convertedPed.thisPed, convertedPed.moveRate);
                }

                if (convertedPed.resetMoveSet) {
                    PedEffects.ResetMoveSet(convertedPed.thisPed);
                    convertedPed.moveSetRequest = default;
                    convertedPed.resetMoveSet = false;
                    convertedPed.hasCustomMoveSet = false;
                } else if (PedEffects.TryRequestMoveSet(convertedPed.moveSetRequest)) {
                    PedEffects.ChangeMoveSet(convertedPed.thisPed, convertedPed.moveSetRequest);
                    convertedPed.moveSetRequest = default;
                    convertedPed.hasCustomMoveSet = true;
                }
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