namespace GunshotWound2.HealthCare {
    using System;
    using GTA;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class SelfHealingSystem : ISystem {
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
                if (health.bleedingWounds != null && health.bleedingWounds.Count > 0) {
                    continue;
                }

                Ped ped = pedStash.Get(entity).thisPed;
                if (ped.Health < health.max) {
                    health.diff += sharedData.mainConfig.WoundConfig.SelfHealingRate * sharedData.deltaTime;
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}