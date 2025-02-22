namespace GunshotWound2.PainFeature.States {
    using Configs;
    using CritsFeature;
    using GTA;
    using GTA.NaturalMotion;
    using PedsFeature;
    using HealthFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class UnbearablePainState : IPainState {
        public const float PAIN_THRESHOLD = 1f;
        private static readonly int[] NM_MESSAGES = { 787, };

        private static readonly string[] NON_PLAYER_DEATH_AMBIENT = {
            "DYING_HELP", "DYING_MOAN", "DYING_PLEAD",
        };

        private static readonly string[] PLAYER_DEATH_AMBIENT = {
            "DEATH_HIGH_LONG", "DEATH_HIGH_MEDIUM", "DEATH_UNDERWATER",
        };

        private static readonly string[] MOODS = {
            "dead_1",
            "dead_2",
            "mood_sleeping_1",
            "mood_knockout_1",
            "pose_aiming_1",
        };

        public float PainThreshold => PAIN_THRESHOLD;
        public string Color => "~r~";

        public void ApplyPainIncreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.Percent() - PainThreshold < 0.1f) {
                pain.diff += 0.2f * pain.max;
            }

            convertedPed.isRestrictToDrive = true;
            SelectVisualBehaviour(sharedData, pedEntity, ref convertedPed);

            int deathAnimIndex = sharedData.random.Next(1, 3);
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"die_{deathAnimIndex.ToString()}", convertedPed.isMale);
            convertedPed.thisPed.StopCurrentPlayingSpeech();

            if (convertedPed.isPlayer) {
                PlayerOnlyCase(sharedData, ref convertedPed);
            } else {
                NonPlayerCase(sharedData, ref convertedPed);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.isRestrictToDrive = false;

            if (convertedPed.isPlayer) {
                SetPlayerIsIgnoredByPeds(sharedData, Game.Player, false);
                sharedData.cameraService.SetUnconsciousEffect(false);
            }
        }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = null;
            return false;
        }

        public bool TryGetMoodSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }

        private static void SelectVisualBehaviour(SharedData sharedData,
                                                  Scellecs.Morpeh.Entity pedEntity,
                                                  ref ConvertedPed convertedPed) {
            if (convertedPed.hasSpineDamage) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due Spine Damage");
#endif
                return;
            }

            Ped ped = convertedPed.thisPed;
            ref Crits crits = ref pedEntity.GetComponent<Crits>();
            bool deadlyCrit = crits.HasActive(Crits.Types.HeartDamaged)
                              || crits.HasActive(Crits.Types.LungsDamaged)
                              || crits.HasActive(Crits.Types.StomachDamaged)
                              || crits.HasActive(Crits.Types.GutsDamaged);

            float totalSeverity = HealthFeature.CalculateSeverityOfAllBleedingWounds(pedEntity);
            float timeToDeath = convertedPed.CalculateTimeToDeath(totalSeverity);
            if (deadlyCrit && timeToDeath <= 20) {
#if DEBUG
                sharedData.logger.WriteInfo("Default writhe as visual behaviour");
#endif
                convertedPed.RemoveRagdollRequest();
                convertedPed.forceRemove = true;

                ped.CanWrithe = true;
                ped.BlockPermanentEvents = true;
                PedEffects.StartWritheTask(ped);
                return;
            }

            const float relaxRagdollPossibility = 0.25f;
            if (sharedData.random.IsTrueWithProbability(relaxRagdollPossibility)) {
#if DEBUG
                sharedData.logger.WriteInfo("Simple ragdoll as visual behaviour");
#endif
                convertedPed.ResetRagdoll();
                convertedPed.RequestPermanentRagdoll();
                return;
            }

#if DEBUG
            sharedData.logger.WriteInfo("InjuredOnGroundHelper as visual behaviour");
#endif
            convertedPed.ResetRagdoll();
            convertedPed.RequestPermanentRagdoll();

            bool armsDamaged = (crits.active & Crits.Types.ArmsDamaged) != 0;
            bool leftSide = sharedData.random.IsTrueWithProbability(0.5f);
            convertedPed.nmHelper = new InjuredOnGroundHelper(ped) {
                Injury1Component = leftSide ? (int)convertedPed.lastDamagedBone : 0,
                Injury2Component = leftSide ? 0 : (int)convertedPed.lastDamagedBone,
                NumInjuries = sharedData.random.Next(0, 3),
                DontReachWithLeft = armsDamaged && leftSide,
                DontReachWithRight = armsDamaged && !leftSide,
                StrongRollForce = sharedData.random.IsTrueWithProbability(0.5f),
            };
        }

        private static void PlayerOnlyCase(SharedData sharedData, ref ConvertedPed convertedPed) {
            string speech = sharedData.random.Next(PLAYER_DEATH_AMBIENT);
            convertedPed.thisPed.PlayAmbientSpeech(speech, SpeechModifier.InterruptShouted);

            Player player = Game.Player;
            if (player.WantedLevel <= 2) {
                SetPlayerIsIgnoredByPeds(sharedData, Game.Player, true);
                if (sharedData.mainConfig.PlayerConfig.PoliceCanForgetYou) {
                    player.WantedLevel = 0;
                }
            }

            if (sharedData.mainConfig.PlayerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            if (!convertedPed.hasSpineDamage) {
                sharedData.notifier.critical.QueueMessage(sharedData.localeConfig.UnbearablePainMessage);
            }

            sharedData.cameraService.SetUnconsciousEffect(true);
        }

        private static void NonPlayerCase(SharedData sharedData, ref ConvertedPed convertedPed) {
            string speech = sharedData.random.Next(NON_PLAYER_DEATH_AMBIENT);
            convertedPed.thisPed.PlayAmbientSpeech(speech, SpeechModifier.InterruptShouted);

            convertedPed.thisPed.Weapons.Drop();

            bool isInVehicle = convertedPed.thisPed.IsInVehicle();
            bool notDriver = convertedPed.thisPed.SeatIndex != VehicleSeat.Driver;
            if (isInVehicle && notDriver && sharedData.random.IsTrueWithProbability(0.5f)) {
                convertedPed.thisPed.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
            }
        }

        private static void SetPlayerIsIgnoredByPeds(SharedData sharedData, Player player, bool value) {
            if (sharedData.mainConfig.PlayerConfig.PedsCanIgnore) {
                player.IgnoredByEveryone = value;
            }
        }
    }
}