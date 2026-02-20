namespace GunshotWound2.StatusFeature.Statuses {
    using GTA;
    using PedsFeature;
    using Utils;

    public sealed class UnconsciousStatus : IPedStatus {
        public const float HEALTH_THRESHOLD = 0.05f;

        public static readonly string[] NON_PLAYER_DEATH_AMBIENT = {
            "DYING_HELP", "DYING_MOAN", "DYING_PLEAD",
        };

        public static readonly string[] PLAYER_DEATH_AMBIENT = {
            "DEATH_HIGH_LONG", "DEATH_HIGH_MEDIUM", "DEATH_UNDERWATER",
        };

        public static readonly string[] DEATH_MOODS = {
            "dead_1",
            "dead_2",
            "mood_sleeping_1",
            "mood_knockout_1",
            "pose_aiming_1",
        };

        private readonly SharedData sharedData;

        public string LocKey => "Status.Unconscious";
        public float PainThreshold => 1f;
        public float HealthThreshold => HEALTH_THRESHOLD;
        public Notifier.Color Color => Notifier.Color.RED;
        public float MoveRate => 0.8f;
        public string[] MaleMoveSets => null;
        public string[] FemaleMoveSets => null;
        public string[] FacialIdleAnims => DEATH_MOODS;
        public string[] PlayerSpeechSet => PLAYER_DEATH_AMBIENT;
        public string[] PedSpeechSet => NON_PLAYER_DEATH_AMBIENT;

        public UnconsciousStatus(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyStatusTo(ref ConvertedPed convertedPed) {
            convertedPed.isRestrictToDrive = true;

            if (convertedPed.isPlayer) {
                PlayerOnlyCase(ref convertedPed);
            } else {
                NonPlayerCase(ref convertedPed);
            }

            if (convertedPed.thisPed.CurrentVehicle.IsValid()) {
                GTAHelpers.PlayDeathAnimationInVehicle(convertedPed.thisPed);
            }
        }

        public void RemoveStatusFrom(ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.isRestrictToDrive = false;

            // PedEffects.StopAnimation(convertedPed.thisPed, convertedPed.forcedAnimation);
            // convertedPed.forcedAnimation = default;
            convertedPed.thisPed.Task.ClearAll();

            if (convertedPed.isPlayer) {
                SetPlayerIsIgnoredByPeds(Game.Player, false);
                sharedData.cameraService.SetUnconsciousEffect(false);
            } else {
                convertedPed.thisPed.BlockPermanentEvents = false;
            }

            convertedPed.thisPed.IsPainAudioEnabled = true;
        }

        private void PlayerOnlyCase(ref ConvertedPed convertedPed) {
            Player player = Game.Player;
            Ped ped = convertedPed.thisPed;
            if (player.Wanted.WantedLevel <= 2) {
                SetPlayerIsIgnoredByPeds(Game.Player, true);
                if (sharedData.mainConfig.playerConfig.PoliceCanForgetYou) {
                    const bool delayLawResponse = false;
                    player.Wanted.SetWantedLevel(0, delayLawResponse);
                    player.Wanted.ApplyWantedLevelChangeNow(delayLawResponse);
                }
            }

            if (sharedData.mainConfig.playerConfig.CanDropWeapon) {
                ped.Weapons.Drop();
            }

            if (!convertedPed.hasSpineDamage) {
                sharedData.notifier.critical.QueueMessage(sharedData.localeConfig.UnbearablePainMessage);
            }

            sharedData.cameraService.SetUnconsciousEffect(true);

            if (PedEffects.OnAnyBike(ped)) {
                PedEffects.KnockOffVehicle(ped);
            }
        }

        private void NonPlayerCase(ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
            ped.BlockPermanentEvents = true;
            ped.Weapons.Drop();

            if (!PedEffects.InAnyVehicle(ped, atGetIn: false)) {
                return;
            }

            if (PedEffects.OnAnyBike(ped)) {
                PedEffects.KnockOffVehicle(ped);
            } else if (ped.SeatIndex != VehicleSeat.Driver && sharedData.random.IsTrueWithProbability(0.5f)) {
                ped.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
            }
        }

        private void SetPlayerIsIgnoredByPeds(Player player, bool value) {
            if (sharedData.mainConfig.playerConfig.PedsWillIgnoreUnconsciousPlayer) {
                player.Wanted.SetEveryoneIgnorePlayer(value);
            }
        }
    }
}