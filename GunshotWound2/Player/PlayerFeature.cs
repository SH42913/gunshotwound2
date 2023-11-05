namespace GunshotWound2.Player {
    using GTA;
    using Scellecs.Morpeh;

    public static class PlayerFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PlayerDetectSystem(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.CheckKey, () => CheckPlayer(sharedData));
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HelmetKey, () => ToggleHelmet(sharedData));
        }

        private static void CheckPlayer(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                HealthChecker.Check(sharedData, playerEntity);
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
                sharedData.notifier.emergency.AddMessage(sharedData.localeConfig.DontHaveMoneyForHelmet);
            }
        }
    }
}