namespace GunshotWound2.HealthCare {
    using System;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class HealthDamageSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter peds;
        private Stash<Health> healthStash;

        public World World { get; set; }

        public HealthDamageSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Health>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (convertedPed.thisPed.IsDead) {
                    continue;
                }
                
                ref Health health = ref healthStash.Get(entity);
                float threshold = convertedPed.isPlayer && convertedPed.IsUsingPhone() ? 5f : 1f;
                if (health.damage < threshold) {
                    continue;
                }

                var currentDamage = (int)Math.Floor(health.damage);
                health.damage -= currentDamage;
                convertedPed.thisPed.Health -= currentDamage;

#if DEBUG
                var healthString = $"Current:{convertedPed.thisPed.Health.ToString()} / Max:{health.max.ToString()}";
                sharedData.logger.WriteInfo($"Dealing {currentDamage.ToString()} damage to {convertedPed.name}. {healthString}");
#endif
            }
        }

        void IDisposable.Dispose() { }
    }
}