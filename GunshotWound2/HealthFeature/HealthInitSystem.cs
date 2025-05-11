namespace GunshotWound2.HealthFeature {
    using Configs;
    using GTA;
    using GTA.Native;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class HealthInitSystem : ISystem {
        private readonly SharedData sharedData;
        private Stash<ConvertedPed> convertedStash;
        private Filter justConvertedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        public HealthInitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            convertedStash = World.GetStash<ConvertedPed>();
            justConvertedPeds = World.Filter.With<ConvertedPed>().With<JustConvertedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            PlayerConfig playerConfig = sharedData.mainConfig.PlayerConfig;
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            foreach (Scellecs.Morpeh.Entity entity in justConvertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                convertedPed.thisPed.CanSufferCriticalHits = false;
                convertedPed.thisPed.DiesOnLowHealth = false;
                convertedPed.thisPed.CanWrithe = false;

                ref Health health = ref entity.AddOrGetComponent<Health>();
                health.max = convertedPed.thisPed.MaxHealth - 1;
                if (convertedPed.thisPed.Health > health.max) {
                    convertedPed.thisPed.Health = health.max;
                } else if (!convertedPed.isPlayer && sharedData.mainConfig.NpcConfig.ScanOnlyDamaged) {
                    convertedPed.thisPed.Health = health.max;
                }

                if (convertedPed.isPlayer) {
                    health.bleedingHealRate = playerConfig.BleedHealingSpeed;
                    Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
                } else {
                    float maxRate = npcConfig.MaximalBleedStopSpeed;
                    float minRate = 0.5f * maxRate;
                    health.bleedingHealRate = sharedData.random.NextFloat(minRate, maxRate);

                    if (npcConfig.MinStartHealth > 0 && npcConfig.MaxStartHealth > 0) {
                        int randomHealth = sharedData.random.Next(npcConfig.MinStartHealth, npcConfig.MaxStartHealth);
                        convertedPed.thisPed.Health = WoundConfig.ConvertHealthToNative(randomHealth);
                    }
                }
            }
        }

        public void Dispose() {
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 1f);
        }
    }
}