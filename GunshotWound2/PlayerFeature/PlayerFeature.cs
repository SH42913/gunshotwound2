namespace GunshotWound2.PlayerFeature {
    using System.Collections.Generic;
    using GTA;
    using PainFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public static class PlayerFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddInitializer(new PlayerInitializer(sharedData));
            systemsGroup.AddSystem(new PlayerDetectSystem(sharedData));
            systemsGroup.AddSystem(new AdrenalineSystem(sharedData));
            systemsGroup.AddSystem(new MedkitGpsSystem(sharedData));
            systemsGroup.AddSystem(new PlayerDeathReportSystem(sharedData));
            systemsGroup.AddSystem(new MissionTrackerSystem(sharedData));
            systemsGroup.AddSystem(new PlayerCameraSystem(sharedData));
            systemsGroup.AddSystem(new ItemPickupSystem(sharedData));
            systemsGroup.AddSystem(new PlayerPainRecoverySystem(sharedData));
            systemsGroup.AddSystem(new PlayerHelpSystem(sharedData));
            systemsGroup.AddSystem(new PlayerSpeedSystem(sharedData));
            systemsGroup.AddSystem(new PlayerHitNotificationSystem(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.CheckSelfKey, () => CheckPlayer(sharedData));
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HelmetKey, () => ToggleHelmet(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.BandagesSelfKey, () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    HealthFeature.HealthFeature.StartBandaging(playerEntity);
                }
            });

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.PainkillersSelfKey, () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity) && !playerEntity.GetComponent<Pain>().TooMuchPain()) {
                    PainFeature.UsePainkillers(playerEntity);
                }
            });

            sharedData.cheatListener.Register("GSW_HEAL", () => {
                Ped playerChar = Game.Player.Character;
                playerChar.Health = playerChar.MaxHealth;
            });
        }

        private static void CheckPlayer(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                sharedData.pedStateService.Check(playerEntity);
            }
        }

        private static void ToggleHelmet(SharedData sharedData) {
            Player player = Game.Player;
            Ped ped = player.Character;
            if (ped.IsRagdoll) {
                return;
            }

            if (ped.IsWearingHelmet) {
                ped.RemoveHelmet(false);
                return;
            }

            PedProp headProp = ped.Style[PedPropAnchorPoint.Head];
            if (headProp.Index != 0) {
                headProp.SetVariation(0);
                return;
            }

            int moneyForHelmet = sharedData.mainConfig.playerConfig.MoneyForHelmet;
            if (player.Money <= 0 || player.Money >= moneyForHelmet) {
                player.Money -= moneyForHelmet;
                GiveRandomHelmet(sharedData, ped, headProp);
            } else {
                sharedData.notifier.ShowOne(sharedData.localeConfig.DontHaveMoneyForHelmet, blinking: true, Notifier.Color.RED);
            }
        }

        private static void GiveRandomHelmet(SharedData sharedData, Ped ped, PedProp headProp) {
            var allHelmets = new List<int>(sharedData.mainConfig.armorConfig.HelmetPropIndexes);
            for (int i = allHelmets.Count - 1; i >= 0; i--) {
                if (!headProp.IsVariationValid(allHelmets[i])) {
                    allHelmets.RemoveAt(i);
                }
            }

            if (allHelmets.Count < 3 || sharedData.modelChecker.IsMainChar(ped.Model)) {
                ped.GiveHelmet(dontTakeOffHelmet: true);
            } else {
                int helmetIndex = sharedData.random.NextFromCollection(allHelmets);

                int textureCount = GTAHelpers.GetPropTextureCount(ped, PedPropAnchorPoint.Head, helmetIndex);
                int textureIndex = sharedData.random.Next(0, textureCount);

                headProp.SetVariation(helmetIndex + 1, textureIndex);
            }
        }
    }
}