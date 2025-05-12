namespace GunshotWound2.HealthFeature {
    using System;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class SelfHealingSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Stash<Health> healthStash;
        private Stash<ConvertedPed> pedStash;
        private Filter peds;

        public Scellecs.Morpeh.World World { get; set; }

        public SelfHealingSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            healthStash = World.GetStash<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            peds = World.Filter.With<Health>().With<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (health.isDead) {
                    continue;
                }

                if (health.HasBleedingWounds()) {
                    continue;
                }

                Ped ped = pedStash.Get(entity).thisPed;
                if (ped.Health + health.diff < health.max) {
                    health.diff += sharedData.mainConfig.woundConfig.SelfHealingRate * sharedData.deltaTime;
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}