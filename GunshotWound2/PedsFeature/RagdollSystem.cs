namespace GunshotWound2.PedsFeature {
    using GTA;
    using GTA.Native;
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
                ragdollRequest.time = newTime > 0 ? newTime : 0;
            } else {
                bool hasNaturalMotion = convertedPed.nmMessages != null;
#if DEBUG
                var time = ragdollRequest.time.ToString();
                sharedData.logger.WriteInfo($"Ragdoll for {convertedPed.name} time={time} hasNM={hasNaturalMotion}");
#endif
                if (hasNaturalMotion) {
                    Function.Call(Hash.SET_PED_TO_RAGDOLL, convertedPed.thisPed.Handle, 10000, ragdollRequest.time, 1, 1, 1, 0);
                    ApplyNaturalMotion(ref convertedPed);
                    convertedPed.nmMessages = null;
                } else {
                    convertedPed.thisPed.Ragdoll(ragdollRequest.time, ragdollRequest.type);
                }

                convertedPed.ragdollRequest = default;
            }
        }

        public void Dispose() {
            foreach (Scellecs.Morpeh.Entity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (convertedPed.thisPed.IsRagdoll) {
                    convertedPed.thisPed.CancelRagdoll();
                }
            }
        }

        private static void ApplyNaturalMotion(ref ConvertedPed convertedPed) {
            PedEffects.StopNaturalMotion(convertedPed.thisPed);
            foreach (int message in convertedPed.nmMessages) {
                PedEffects.SetNaturalMotionMessage(convertedPed.thisPed, message);
            }
        }
    }
}