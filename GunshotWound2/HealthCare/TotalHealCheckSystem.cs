﻿namespace GunshotWound2.HealthCare {
    using System;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class TotalHealCheckSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;
        private Stash<Health> healthStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;

        public World World { get; set; }

        public TotalHealCheckSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            healthStash = World.GetStash<Health>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                if (totallyHealedStash.Remove(entity)) {
                    continue;
                }
                
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref Health health = ref healthStash.Get(entity);
                if (convertedPed.thisPed.Health > health.max) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Ped {convertedPed.name} was totally healed");
#endif
                    //TODO: Notification?
                    convertedPed.thisPed.Health = health.max;
                    entity.SetMarker<TotallyHealedEvent>();
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}