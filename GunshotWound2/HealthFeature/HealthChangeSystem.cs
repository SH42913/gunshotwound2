namespace GunshotWound2.HealthFeature {
    using System;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class HealthChangeSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter peds;
        private Stash<Health> healthStash;

        public World World { get; set; }

        public HealthChangeSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Health>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (health.isDead) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (convertedPed.thisPed.IsDead) {
                    health.isDead = true;
                    CreateDeathReport(entity, ref convertedPed, ref health);
                    continue;
                }

                if (health.kill) {
                    convertedPed.thisPed.Health -= 100;
                    continue;
                }

                float threshold = convertedPed.isPlayer && convertedPed.IsUsingPhone() ? 5f : 1f;
                if (Math.Abs(health.diff) < threshold) {
                    continue;
                }

                int currentDiff = health.diff > 0 ? (int)Math.Floor(health.diff) : (int)Math.Ceiling(health.diff);
                health.diff -= currentDiff;

                int oldHealth = convertedPed.thisPed.Health;
                int newHealth = oldHealth + currentDiff;
                convertedPed.thisPed.Health = newHealth;

                health.isDead = Configs.WoundConfig.ConvertHealthFromNative(newHealth) < 0;
                CreateDeathReport(entity, ref convertedPed, ref health);

#if DEBUG
                var healthString = $"{oldHealth.ToString()} -> {newHealth.ToString()} / Max:{health.max.ToString()}";
                sharedData.logger.WriteInfo($"Changed health: {currentDiff.ToString()} to {convertedPed.name}. {healthString}");
#endif
            }
        }

        private void CreateDeathReport(Entity entity, ref ConvertedPed convertedPed, ref Health health) {
            if (health.isDead && convertedPed.isPlayer) {
                World.CreateEntity().SetComponent(new ShowPlayerDeathReportRequest {
                    report = PedStateChecker.BuildString(sharedData, entity),
                });
            }
        }

        void IDisposable.Dispose() { }
    }
}