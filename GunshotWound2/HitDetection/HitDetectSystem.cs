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
            convertedPeds = World.Filter.With<ConvertedPed>().Without<JustConvertedEvent>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in convertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (!ped.Exists() || !ped.IsAlive) {
                    continue;
                }

                int healthDiff = convertedPed.lastFrameHealth - ped.Health;
                int armorDiff = convertedPed.lastFrameArmor - ped.Armor;
                if (healthDiff <= 0 && armorDiff <= 0) {
                    continue;
                }

                entity.SetComponent(new PedHitData {
                    healthDiff = healthDiff,
                    armorDiff = armorDiff,
                });

#if DEBUG
                sharedData.logger.WriteInfo($"Detect damage at {convertedPed.name}");
                sharedData.logger.WriteInfo($"healthDiff = {healthDiff.ToString()}, armorDiff = {armorDiff.ToString()}");
#endif
            }
        }
    }
}