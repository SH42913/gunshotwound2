namespace GunshotWound2.HealthFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class SelfHealingSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter peds;
        private AspectFactory<PedWithHealth> pedWithHealthAspect;

        public EcsWorld World { get; set; }

        public SelfHealingSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.Extend<PedWithHealth>();
            pedWithHealthAspect = World.GetAspectFactory<PedWithHealth>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                PedWithHealth aspect = pedWithHealthAspect.Get(entity);
                ref Health health = ref aspect.Health;
                if (health.isDead) {
                    continue;
                }

                if (health.HasBleedingWounds()) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref aspect.Ped;
                if (convertedPed.thisPed.Health + health.diff < health.max) {
                    float selfHealingRate = convertedPed.isPlayer
                            ? sharedData.mainConfig.playerConfig.SelfHealingRate
                            : sharedData.mainConfig.pedsConfig.SelfHealingRate;

                    health.diff += selfHealingRate * sharedData.deltaTime;
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}