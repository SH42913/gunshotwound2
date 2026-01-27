namespace GunshotWound2.PedsFeature {
    using Configs;
    using Scellecs.Morpeh;
    using Utils;

    public static class PedsFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PedDamageModifierSystem(sharedData));
            systemsGroup.AddSystem(new NpcDetectSystem(sharedData));
            systemsGroup.AddSystem(new ConvertPedSystem(sharedData));
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
                if (sharedData.TryGetPlayer(out Entity entity)) {
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
                GTA.Ped ped = GTA.World.CreateRandomPed(GTA.Game.Player.Character.BelowPosition);
                ped.DecisionMaker = new GTA.DecisionMaker(GTA.DecisionMakerTypeHash.Empty);
                ped.SetConfigFlag(GTA.PedConfigFlagToggles.DontActivateRagdollFromBulletImpact, true);
            });
        }

        private static void CheckClosestPed(SharedData sharedData) {
            if (sharedData.TryGetClosestPedEntity(out _, out Entity entity)) {
                sharedData.pedStateService.Check(entity);
            }
        }

        private static void BandageClosestPed(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out Entity playerEntity) && sharedData.TryGetClosestPedEntity(out _, out Entity entity)) {
                HealthFeature.HealthFeature.StartBandaging(entity, medic: playerEntity);
            }
        }

        private static void PainkillersToClosestPed(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out Entity playerEntity) && sharedData.TryGetClosestPedEntity(out _, out Entity entity)) {
                PainFeature.PainFeature.UsePainkillers(entity, medic: playerEntity);
            }
        }
    }
}