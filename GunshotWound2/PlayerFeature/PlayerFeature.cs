namespace GunshotWound2.PlayerFeature {
    using GTA;
    using Scellecs.Morpeh;
    using Utils;

    public static class PlayerFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PlayerDetectSystem(sharedData));
            systemsGroup.AddSystem(new AdrenalineSystem(sharedData));
            systemsGroup.AddSystem(new MedkitGpsSystem(sharedData));
            systemsGroup.AddSystem(new PlayerDeathReportSystem(sharedData));
            systemsGroup.AddSystem(new MissionTrackerSystem(sharedData));
            systemsGroup.AddSystem(new PlayerCameraSystem(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.CheckSelfKey, () => CheckPlayer(sharedData));
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HelmetKey, () => ToggleHelmet(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.BandageSelfKey, () => {
                if (sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                    HealthFeature.HealthFeature.StartBandaging(playerEntity);
                }
            });

            sharedData.cheatListener.Register("GSW_HEAL", () => {
                Ped playerChar = Game.Player.Character;
                playerChar.Health = playerChar.MaxHealth;
            });
        }

        private static void CheckPlayer(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
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

            int moneyForHelmet = sharedData.mainConfig.PlayerConfig.MoneyForHelmet;
            if (player.Money <= 0 || player.Money >= moneyForHelmet) {
                player.Money -= moneyForHelmet;
                int helmetTextureId = sharedData.random.Next(0, 15);
                ped.GiveHelmet(dontTakeOffHelmet: true, HelmetPropFlags.DefaultHelmet, helmetTextureId);
            } else {
                sharedData.notifier.ShowOne(sharedData.localeConfig.DontHaveMoneyForHelmet, blinking: true, Notifier.Color.RED);
            }
        }
    }
}