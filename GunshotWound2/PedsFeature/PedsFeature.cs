namespace GunshotWound2.PedsFeature {
    using System.Windows.Forms;
    using Configs;
    using Scellecs.Morpeh;
    using Utils;

    public static class PedsFeature {
        private static int LAST_POST;

        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new NpcDetectSystem(sharedData));
            systemsGroup.AddSystem(new ConvertPedSystem(sharedData));
            systemsGroup.AddSystem(new RagdollSystem(sharedData));
            systemsGroup.AddSystem(new PedMovementSystem(sharedData));
            systemsGroup.AddSystem(new RemoveConvertedPedSystem(sharedData));

            MainConfig mainConfig = sharedData.mainConfig;
            InputListener inputListener = sharedData.inputListener;
            inputListener.RegisterHotkey(mainConfig.IncreaseRangeKey, () => ChangeRange(sharedData, 5f));
            inputListener.RegisterHotkey(mainConfig.DecreaseRangeKey, () => ChangeRange(sharedData, -5f));
            inputListener.RegisterHotkey(mainConfig.BandageKey, () => BandageClosestPed(sharedData), Keys.Shift);
            inputListener.RegisterHotkey(mainConfig.CheckKey, () => CheckClosestPed(sharedData), Keys.Shift);

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
            });
        }

        private static void ChangeRange(SharedData sharedData, float value) {
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            if (npcConfig.AddingPedRange + value < MainConfig.MINIMAL_RANGE_FOR_WOUNDED_PEDS) {
                return;
            }

            npcConfig.AddingPedRange += value;
            npcConfig.RemovePedRange = npcConfig.AddingPedRange * MainConfig.ADDING_TO_REMOVING_MULTIPLIER;

            LocaleConfig localeConfig = sharedData.localeConfig;
            var scan = npcConfig.AddingPedRange.ToString("F0");
            var remove = npcConfig.RemovePedRange.ToString("F0");
            LAST_POST = sharedData.notifier.ReplaceOne($"{localeConfig.AddingRange}: {scan}\n{localeConfig.RemovingRange}: {remove}",
                                                       blinking: false, LAST_POST);
        }

        private static void CheckClosestPed(SharedData sharedData) {
            if (TryGetClosestPed(out GTA.Ped closest) && sharedData.worldService.TryGetConverted(closest, out Entity entity)) {
                PedStateChecker.Check(sharedData, entity);
            }
        }

        private static void BandageClosestPed(SharedData sharedData) {
            if (!TryGetClosestPed(out GTA.Ped closest) || !sharedData.worldService.TryGetConverted(closest, out Entity entity)) {
                return;
            }

            if (!sharedData.TryGetPlayer(out Entity playerEntity)) {
                return;
            }

            entity.SetComponent(new HealthFeature.BandageRequest {
                medic = playerEntity,
            });
        }

        private static bool TryGetClosestPed(out GTA.Ped closestPed) {
            const float radius = 2f;
            GTA.Ped playerPed = GTA.Game.Player.Character;
            GTA.Ped[] closestPeds = GTA.World.GetNearbyPeds(playerPed, radius);
            if (closestPeds.Length > 0) {
                closestPed = closestPeds[0];
                return true;
            } else {
                closestPed = null;
                return false;
            }
        }
    }
}