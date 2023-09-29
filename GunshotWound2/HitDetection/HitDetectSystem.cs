namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class HitDetectSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter convertedPeds;
        private Stash<ConvertedPed> convertedStash;

        public Scellecs.Morpeh.World World { get; set; }

        public HitDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            convertedPeds = World.Filter.With<ConvertedPed>().Without<JustConvertedMarker>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in convertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;

                int healthDiff = convertedPed.lastFrameHealth - ped.Health;
                if (ped.Exists() && ped.IsAlive && healthDiff > 0) {
                    entity.SetComponent(new PedHitData {
                        healthDiff = healthDiff,
                    });

#if DEBUG
                    sharedData.logger.WriteInfo($"Detect damage at {convertedPed.name}, healthDiff = {healthDiff.ToString()}");
#endif
                }
            }
        }
    }
}