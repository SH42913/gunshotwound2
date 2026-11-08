namespace GunshotWound2.HealthFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class TotalHealCheckSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter peds;
        private AspectFactory<PedWithHealth> pedWithHealthAspect;
        private Stash<TotallyHealedEvent> totallyHealedStash;

        public EcsWorld World { get; set; }

        public TotalHealCheckSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.Extend<PedWithHealth>();
            pedWithHealthAspect = World.GetAspectFactory<PedWithHealth>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                bool wasHealedLastFrame = totallyHealedStash.Remove(entity);
                if (wasHealedLastFrame) {
                    continue;
                }

                PedWithHealth aspect = pedWithHealthAspect.Get(entity);
                ref ConvertedPed convertedPed = ref aspect.Ped;
                ref Health health = ref aspect.Health;
                if (health.IsAlive() && convertedPed.thisPed.Health > health.max) {
#if DEBUG && DEBUG_EVERY_FRAME
                    sharedData.logger.WriteInfo($"Ped {convertedPed.name} was totally healed");
#endif
                    if (convertedPed.isPlayer) {
                        sharedData.notifier.info.QueueMessage(sharedData.localeConfig.TotallyHealedMessage, Notifier.Color.GREEN);
                    }

                    convertedPed.SetHealth(health.max);
                    convertedPed.thisPed.ClearBloodDamage();
                    entity.SetMarker<TotallyHealedEvent>();
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}