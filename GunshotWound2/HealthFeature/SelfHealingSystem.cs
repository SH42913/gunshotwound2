namespace GunshotWound2.HealthFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class SelfHealingSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Stash<Health> healthStash;
        private Stash<ConvertedPed> pedStash;
        private Filter peds;

        public World World { get; set; }

        public SelfHealingSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            healthStash = World.GetStash<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            peds = World.Filter.With<Health>().With<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (health.isDead) {
                    continue;
                }

                if (health.HasBleedingWounds()) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
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