namespace GunshotWound2.PedsFeature {
    using Configs;
    using GTA;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

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

            MainConfig mainConfig = sharedData.mainConfig;
            InputListener inputListener = sharedData.inputListener;
            inputListener.RegisterHotkey(mainConfig.CheckClosestKey, () => CheckClosestPed(sharedData));
            inputListener.RegisterHotkey(mainConfig.BandagesClosestKey, () => BandageClosestPed(sharedData));
            inputListener.RegisterHotkey(mainConfig.PainkillersClosestKey, () => PainkillersToClosestPed(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_TEST_RAGDOLL", () => {
                if (sharedData.TryGetPlayer(out EcsEntity entity)) {
                    ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                    if (convertedPed.thisPed.IsRagdoll) {
                        convertedPed.ResetRagdoll();
                    } else {
                        convertedPed.RequestPermanentRagdoll();
                    }
                }
            });
#endif

            sharedData.cheatListener.Register("GSW_TEST_PED", () => {
#if SHVDN_NIGHTLY
                Ped ped = Ped.CreateRandom(Game.Player.Character.BelowPosition);
#else
                Ped ped = GTA.World.CreateRandomPed(Game.Player.Character.BelowPosition);
#endif
                ped.DecisionMaker = new DecisionMaker(DecisionMakerTypeHash.Empty);
                ped.SetConfigFlag(PedConfigFlagToggles.DontActivateRagdollFromBulletImpact, true);
            });
        }

        private static void CheckClosestPed(SharedData sharedData) {
            if (sharedData.TryGetClosestPedEntity(out _, out EcsEntity entity)) {
                sharedData.pedStateService.Check(entity);
            }
        }

        private static void BandageClosestPed(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out EcsEntity playerEntity)
                && sharedData.TryGetClosestPedEntity(out _, out EcsEntity entity)) {
                HealthFeature.HealthFeature.StartBandaging(entity, medic: playerEntity);
            }
        }

        private static void PainkillersToClosestPed(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out EcsEntity playerEntity)
                && sharedData.TryGetClosestPedEntity(out _, out EcsEntity entity)) {
                PainFeature.PainFeature.UsePainkillers(entity, medic: playerEntity);
            }
        }
    }
}