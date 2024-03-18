﻿namespace GunshotWound2.PlayerFeature {
    using GTA;
    using Scellecs.Morpeh;

    public static class PlayerFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PlayerDetectSystem(sharedData));
            systemsGroup.AddSystem(new AdrenalineSystem(sharedData));
            systemsGroup.AddSystem(new MedkitGpsSystem(sharedData));
            systemsGroup.AddSystem(new PlayerDeathReportSystem());

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.CheckKey, () => CheckPlayer(sharedData));
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HelmetKey, () => ToggleHelmet(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.BandageKey, () => {
                if (sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                    HealthFeature.BandageSystem.TryToBandage(sharedData, playerEntity);
                }
            });

            sharedData.cheatListener.Register("GSW_HEAL", () => {
                Ped playerChar = Game.Player.Character;
                playerChar.Health = playerChar.MaxHealth;
            });
        }

        private static void CheckPlayer(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                PedStateChecker.Check(sharedData, playerEntity);
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

            int moneyForHelmet = sharedData.mainConfig.PlayerConfig.MoneyForHelmet;
            if (player.Money >= moneyForHelmet) {
                player.Money -= moneyForHelmet;
                ped.GiveHelmet(false, Helmet.RegularMotorcycleHelmet, sharedData.random.Next(0, 15));
            } else {
                sharedData.notifier.emergency.QueueMessage(sharedData.localeConfig.DontHaveMoneyForHelmet);
            }
        }
    }
}