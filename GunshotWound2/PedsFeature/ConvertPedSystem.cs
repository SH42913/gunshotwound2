namespace GunshotWound2.PedsFeature {
    using System;
    using Configs;
    using GTA;
    using Scellecs.Morpeh;
    using Services;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class ConvertPedSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter justConverted;

        public EcsWorld World { get; set; }

        public ConvertPedSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            justConverted = World.Filter.With<JustConvertedEvent>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in justConverted) {
                entity.RemoveComponent<JustConvertedEvent>();
            }

            WorldService worldService = sharedData.worldService;
            while (worldService.TryGetToConvert(out Ped pedToConvert)) {
                if (!pedToConvert.IsValid()) {
                    continue;
                }

                EcsEntity entity = World.CreateEntity();

                ref ConvertedPed convertedPed = ref entity.AddComponent<ConvertedPed>();
                convertedPed.thisPed = pedToConvert;
                convertedPed.isMale = pedToConvert.Gender == Gender.Male;
                convertedPed.defaultMaxHealth = pedToConvert.MaxHealth;
                convertedPed.name = $"Ped_{pedToConvert.Handle.ToString()}";

                PedsConfig pedsConfig = sharedData.mainConfig.pedsConfig;
                convertedPed.combatPerformanceMult = 1f;
                convertedPed.accuracyBase = -1;
                convertedPed.shootRateBase = -1;

                if (pedsConfig.DontActivateRagdollFromBulletImpact) {
                    convertedPed.thisPed.SetConfigFlag(PedConfigFlagToggles.DontActivateRagdollFromBulletImpact, true);
                }

                worldService.AddConverted(pedToConvert, entity);
                entity.AddComponent<JustConvertedEvent>();

#if PED_BLIPS
                convertedPed.customBlip = pedToConvert.AddBlip();
                convertedPed.customBlip.Scale = 0.3f;
#endif
            }
        }
    }
}