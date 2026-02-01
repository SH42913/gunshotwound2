namespace GunshotWound2.PedsFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PedCombatPerfSystem : ISystem {
        private readonly SharedData sharedData;

        public EcsWorld World { get; set; }

        private Filter peds;
        private Stash<ConvertedPed> pedsStash;

        private PedsConfig PedsConfig => sharedData.mainConfig.pedsConfig;

        public PedCombatPerfSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedsStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                ref ConvertedPed convertedPed = ref pedsStash.Get(entity);
                if (!convertedPed.thisPed.IsInCombat) {
                    continue;
                }

                UpdateAccuracy(ref convertedPed);
                UpdateShootRate(ref convertedPed);
            }
        }

        void IDisposable.Dispose() { }

        private void UpdateAccuracy(ref ConvertedPed convertedPed) {
            if (convertedPed.accuracyBase < 0) {
                int pedAccuracy = convertedPed.thisPed.Accuracy;
                convertedPed.accuracyBase = PedsConfig.CustomAccuracyEnabled && pedAccuracy > PedsConfig.MinAccuracy
                        ? sharedData.random.Next(PedsConfig.MinAccuracy, PedsConfig.MaxAccuracy + 1)
                        : pedAccuracy;
            }

            convertedPed.thisPed.Accuracy = (int)(convertedPed.combatPerformanceMult * convertedPed.accuracyBase);
        }

        private void UpdateShootRate(ref ConvertedPed convertedPed) {
            if (PedsConfig.CustomShootRateEnabled && convertedPed.shootRateBase < 0) {
                convertedPed.shootRateBase = sharedData.random.Next(PedsConfig.MinShootRate, PedsConfig.MaxShootRate + 1);
            }

            if (convertedPed.shootRateBase > 0) {
                convertedPed.thisPed.ShootRate = (int)(convertedPed.combatPerformanceMult * convertedPed.shootRateBase);
            }
        }
    }
}