namespace GunshotWound2.HealthCare {
    using System;
    using Peds;
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
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (convertedPed.thisPed.IsDead) {
                    continue;
                }

                ref Health health = ref healthStash.Get(entity);
                float threshold = convertedPed.isPlayer && convertedPed.IsUsingPhone() ? 5f : 1f;
                if (Math.Abs(health.diff) < threshold) {
                    continue;
                }

                int currentDiff = health.diff > 0 ? (int)Math.Floor(health.diff) : (int)Math.Ceiling(health.diff);
                health.diff -= currentDiff;
                convertedPed.thisPed.Health += currentDiff;

#if DEBUG
                var healthString = $"Current:{convertedPed.thisPed.Health.ToString()} / Max:{health.max.ToString()}";
                sharedData.logger.WriteInfo($"Changed health: {currentDiff.ToString()} to {convertedPed.name}. {healthString}");
#endif
            }
        }

        void IDisposable.Dispose() { }
    }
}