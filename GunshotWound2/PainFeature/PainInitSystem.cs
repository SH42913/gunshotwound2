namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class PainInitSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter justConvertedPeds;

        public World World { get; set; }

        public PainInitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            justConvertedPeds = World.Filter.With<ConvertedPed>().With<JustConvertedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            PlayerConfig playerConfig = sharedData.mainConfig.playerConfig;
            PedsConfig pedsConfig = sharedData.mainConfig.pedsConfig;
            foreach (Entity entity in justConvertedPeds) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                ref Pain pain = ref entity.AddOrGetComponent<Pain>();
                if (convertedPed.isPlayer) {
                    pain.recoveryRate = playerConfig.PainRecoverySpeed;
                    pain.max = playerConfig.PainShockThreshold;
                } else {
                    float maxRate = pedsConfig.MaximalPainRecoverSpeed;
                    float minRate = 0.5f * maxRate;
                    pain.recoveryRate = sharedData.random.NextFloat(minRate, maxRate);
                    pain.max = sharedData.random.NextFloat(pedsConfig.MinPainShockThreshold, pedsConfig.MaxPainShockThreshold);
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}