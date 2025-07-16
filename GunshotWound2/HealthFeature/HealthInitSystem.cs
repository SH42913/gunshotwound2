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
            PlayerConfig playerConfig = sharedData.mainConfig.playerConfig;
            PedsConfig pedsConfig = sharedData.mainConfig.pedsConfig;
            foreach (Scellecs.Morpeh.Entity entity in justConvertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                ped.CanSufferCriticalHits = false;
                ped.DiesOnLowHealth = false;
                ped.CanWrithe = false;

                ped.InjuryHealthThreshold = StatusFeature.Statuses.CriticalStatus.HEALTH_THRESHOLD;
                ped.FatalInjuryHealthThreshold = StatusFeature.Statuses.UnconsciousStatus.HEALTH_THRESHOLD;

                ref Health health = ref entity.AddOrGetComponent<Health>();
                health.max = convertedPed.defaultMaxHealth;
                ped.MaxHealth = health.max + 1;
                if (ped.Health > health.max) {
                    ped.Health = health.max;
                } else if (!convertedPed.isPlayer && sharedData.mainConfig.pedsConfig.ScanOnlyDamaged) {
                    ped.Health = health.max;
                }

                if (convertedPed.isPlayer) {
                    health.bleedingHealRate = playerConfig.BleedHealingSpeed;
                    Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
                } else {
                    float maxRate = pedsConfig.MaximalBleedStopSpeed;
                    float minRate = 0.5f * maxRate;
                    health.bleedingHealRate = sharedData.random.NextFloat(minRate, maxRate);

                    if (pedsConfig.MinStartHealth > 0 && pedsConfig.MaxStartHealth > 0) {
                        int randomHealth = sharedData.random.Next(pedsConfig.MinStartHealth, pedsConfig.MaxStartHealth);
                        ped.Health = WoundConfig.ConvertHealthToNative(randomHealth);
                    }
                }
            }
        }

        public void Dispose() {
            Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 1f);
        }
    }
}