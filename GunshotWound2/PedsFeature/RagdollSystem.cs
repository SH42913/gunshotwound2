namespace GunshotWound2.PedsFeature {
    using GTA;
    using Scellecs.Morpeh;

    public sealed class RagdollSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;

        public Scellecs.Morpeh.World World { get; set; }

        public RagdollSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in peds) {
                Process(ref pedStash.Get(entity));
            }
        }

        private void Process(ref ConvertedPed convertedPed) {
            bool inRagdoll = convertedPed.thisPed.IsRagdoll;
            if (convertedPed.ragdollReset) {
                if (inRagdoll) {
                    convertedPed.thisPed.CancelRagdoll();
                }

                convertedPed.ragdollRequest = default;
                convertedPed.ragdollReset = default;
                return;
            }

            ref (int time, RagdollType type) ragdollRequest = ref convertedPed.ragdollRequest;
            if (ragdollRequest.time == 0) {
                return;
            }

            if (inRagdoll) {
                if (ragdollRequest.time < 0) {
                    return;
                }

                int newTime = ragdollRequest.time - sharedData.deltaTimeInMs;
                ragdollRequest.time = newTime > 0
                        ? newTime
                        : 0;
            } else {
                convertedPed.thisPed.Ragdoll(ragdollRequest.time, ragdollRequest.type);
                convertedPed.ragdollRequest = default;
            }
        }

        public void Dispose() { }
    }
}