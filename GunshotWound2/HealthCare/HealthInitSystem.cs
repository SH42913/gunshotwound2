namespace GunshotWound2.HealthCare {
    using System;
    using Configs;
    using GTA;
    using GTA.Native;
    using Peds;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class HealthInitSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter peds;

        public Scellecs.Morpeh.World World { get; set; }

        public HealthInitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<JustConvertedMarker>();
        }

        public void OnUpdate(float deltaTime) {
            PlayerConfig playerConfig = sharedData.mainConfig.PlayerConfig;
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            foreach (Scellecs.Morpeh.Entity entity in peds) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                convertedPed.thisPed.CanSufferCriticalHits = true;
                convertedPed.thisPed.DiesOnLowHealth = false;
                convertedPed.thisPed.CanWrithe = false;

                ref Health health = ref entity.AddOrGetComponent<Health>();
                health.max = convertedPed.thisPed.MaxHealth - 1;

                if (convertedPed.isPlayer) {
                    health.bleedingHealRate = playerConfig.BleedHealingSpeed;
                    Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
                } else {
                    float maxRate = npcConfig.MaximalBleedStopSpeed;
                    float minRate = 0.5f * maxRate;
                    health.bleedingHealRate = sharedData.random.NextFloat(minRate, maxRate);
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}