namespace GunshotWound2.HealthCare {
    using System;
    using Configs;
    using Peds;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class HealthInitSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter peds;

        public World World { get; set; }

        public HealthInitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<JustConvertedMarker>();
        }

        public void OnUpdate(float deltaTime) {
            PlayerConfig playerConfig = sharedData.mainConfig.PlayerConfig;
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            foreach (Entity entity in peds) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                convertedPed.thisPed.CanSufferCriticalHits = true;
                convertedPed.thisPed.DiesOnLowHealth = false;
                convertedPed.thisPed.CanWrithe = false;

                ref Health health = ref entity.AddOrGetComponent<Health>();
                health.max = convertedPed.thisPed.MaxHealth - 1;
                health.bleedingHealRate = convertedPed.isPlayer
                        ? playerConfig.BleedHealingSpeed
                        : sharedData.random.NextFloat(0.5f * npcConfig.MaximalBleedStopSpeed, npcConfig.MaximalBleedStopSpeed);
            }
        }

        void IDisposable.Dispose() { }
    }
}