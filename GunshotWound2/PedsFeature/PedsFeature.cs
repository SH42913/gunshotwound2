namespace GunshotWound2.PedsFeature {
    using Compat;
    using Configs;
    using GTA;
    using HealthFeature;
    using PainFeature;
    using Scellecs.Morpeh;
    using Utils;

    public static class PedsFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new NpcDetectSystem(sharedData));
            systemsGroup.AddSystem(new ConvertPedSystem(sharedData));
            systemsGroup.AddSystem(new PedCombatPerfSystem(sharedData));
            systemsGroup.AddSystem(new RagdollSystem(sharedData));
            systemsGroup.AddSystem(new InjuredOnGroundUpdateSystem(sharedData));
            systemsGroup.AddSystem(new PedMovementSystem(sharedData));
            systemsGroup.AddSystem(new FacialIdleAnimSystem(sharedData));
            systemsGroup.AddSystem(new RemoveConvertedPedSystem(sharedData));
            systemsGroup.AddSystem(new WeaponDropSystem(sharedData));

            MainConfig mainConfig = sharedData.mainConfig;
            InputListener inputListener = sharedData.inputListener;
            inputListener.RegisterHotkey(mainConfig.CheckClosestKey, () => CheckClosestPed(sharedData));
            inputListener.RegisterHotkey(mainConfig.BandagesClosestKey, () => BandageClosestPed(sharedData));
            inputListener.RegisterHotkey(mainConfig.PainkillersClosestKey, () => PainkillersToClosestPed(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_TEST_RAGDOLL", () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    ref ConvertedPed convertedPed = ref playerEntity.GetComponent<ConvertedPed>();
                    if (convertedPed.thisPed.IsRagdoll) {
                        convertedPed.ResetRagdoll();
                    } else {
                        convertedPed.RequestPermanentRagdoll();
                    }
                }
            });
#endif

            sharedData.cheatListener.Register("GSW_TEST_PED", () => {
                Ped ped = ShvdnCompat.CreateRandomPed(Game.Player.Character.BelowPosition);
                ped.DecisionMaker = new DecisionMaker(DecisionMakerTypeHash.Empty);
                ped.SetConfigFlag(PedConfigFlagToggles.DontActivateRagdollFromBulletImpact, true);
            });
        }

        private static void CheckClosestPed(SharedData sharedData) {
            if (sharedData.TryGetClosestPedEntity(out _, out EcsEntity pedEntity)) {
                sharedData.pedStateService.Check(pedEntity);
            }
        }

        private static void BandageClosestPed(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out EcsEntity playerEntity)
                && sharedData.TryGetClosestPedEntity(out _, out EcsEntity pedEntity)) {
                HealthFeature.StartBandaging(pedEntity, medic: playerEntity);
            }
        }

        private static void PainkillersToClosestPed(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out EcsEntity playerEntity)
                && sharedData.TryGetClosestPedEntity(out _, out EcsEntity pedEntity)) {
                PainFeature.UsePainkillers(pedEntity, medic: playerEntity);
            }
        }
    }
}