namespace GunshotWound2.PainFeature.States {
    using System;
    using Configs;
    using CritsFeature;
    using GTA;
    using GTA.Math;
    using GTA.NaturalMotion;
    using PedsFeature;
    using HealthFeature;
    using Scellecs.Morpeh;
    using Utils;
    using Weighted_Randomizer;

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

        private readonly ConvertedPed.AfterRagdollAction writheAction;
        private readonly ConvertedPed.AfterRagdollAction crawlAction;

        public UnbearablePainState() {
            writheAction = StartWrithe;
            crawlAction = StartCrawl;
        }

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

        private void SelectVisualBehaviour(SharedData sharedData,
                                           Scellecs.Morpeh.Entity pedEntity,
                                           ref ConvertedPed convertedPed) {
            if (convertedPed.hasSpineDamage) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due Spine Damage");
#endif
                return;
            }

            IWeightedRandomizer<int> randomizer = sharedData.weightRandom;
            randomizer.Clear();
            randomizer.Add(0);
            randomizer.Add(1, 3);

            ref Crits crits = ref pedEntity.GetComponent<Crits>();
            bool armsDamaged = crits.HasActive(Crits.Types.ArmsDamaged);
            bool legsDamaged = crits.HasActive(Crits.Types.LegsDamaged);
            bool heavyCrit = crits.HasActive(Crits.Types.HeartDamaged)
                             || crits.HasActive(Crits.Types.LungsDamaged)
                             || crits.HasActive(Crits.Types.StomachDamaged)
                             || crits.HasActive(Crits.Types.GutsDamaged);

            if (heavyCrit || legsDamaged) {
                randomizer.Add(2, 2);
            }

            float totalSeverity = HealthFeature.CalculateSeverityOfAllBleedingWounds(pedEntity);
            float timeToDeath = convertedPed.CalculateTimeToDeath(totalSeverity);
            if (heavyCrit && timeToDeath <= 30f) {
                randomizer.Add(3, 2);
            }

            switch (randomizer.NextWithReplacement()) {
                case 0:  SimpleRagdollVisualBehaviour(sharedData, ref convertedPed); break;
                case 1:  InjuredVisualBehaviour(sharedData, ref convertedPed, armsDamaged); break;
                case 2:  CrawlVisualBehaviour(sharedData, ref convertedPed); break;
                case 3:  WritheVisualBehaviour(sharedData, ref convertedPed); break;
                default: throw new Exception("Incorrect visual behaviour index");
            }
        }

        private static void SimpleRagdollVisualBehaviour(SharedData sharedData, ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Simple ragdoll as visual behaviour");
#endif
            convertedPed.ResetRagdoll();
            convertedPed.RequestPermanentRagdoll();
        }

        private void WritheVisualBehaviour(SharedData sharedData, ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Default writhe as visual behaviour");
#endif
            convertedPed.RequestRagdoll(3000);
            convertedPed.afterRagdollAction = writheAction;
        }

        private void CrawlVisualBehaviour(SharedData sharedData, ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Crawl animation as visual behaviour");
#endif
            convertedPed.RequestRagdoll(1000);
            convertedPed.afterRagdollAction = crawlAction;
        }

        private static void InjuredVisualBehaviour(SharedData sharedData, ref ConvertedPed convertedPed, bool armsDamaged) {
#if DEBUG
            sharedData.logger.WriteInfo("InjuredOnGroundHelper as visual behaviour");
#endif
            convertedPed.ResetRagdoll();
            convertedPed.RequestPermanentRagdoll();

            PedEffects.DetermineBoneSide(convertedPed.lastDamagedBone, out bool leftSide, out bool rightSide);
            var helper = new InjuredOnGroundHelper(convertedPed.thisPed) {
                Injury1Component = (int)convertedPed.lastDamagedBone,
                NumInjuries = sharedData.random.Next(1, 3),
                DontReachWithLeft = armsDamaged && leftSide,
                DontReachWithRight = armsDamaged && rightSide,
                StrongRollForce = sharedData.random.IsTrueWithProbability(0.5f),
            };

            if (PedEffects.TryGetLastDamageRecord(convertedPed.thisPed, out _, out int handle)) {
                helper.AttackerPos = GTA.Entity.FromHandle(handle).Position;
            }

            convertedPed.nmHelper = helper;
        }

        private static void StartWrithe(ref ConvertedPed convertedPed) {
            convertedPed.forceRemove = true;

            Ped ped = convertedPed.thisPed;
            ped.Task.ClearAllImmediately();
            ped.CanWrithe = true;
            ped.BlockPermanentEvents = true;

            if (PedEffects.TryGetLastDamageRecord(ped, out _, out int attackerHandle)) {
                PedEffects.StartWritheTask(ped, (Ped)GTA.Entity.FromHandle(attackerHandle));
            } else {
                PedEffects.StartWritheTask(ped);
            }
        }

        private static void StartCrawl(ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
            ped.BlockPermanentEvents = true;

            bool back;
            Quaternion quat;
            if (PedEffects.TryGetLastDamageRecord(ped, out _, out int attackerHandle)) {
                GTA.Entity attacker = GTA.Entity.FromHandle(attackerHandle);
                Vector3 dir = ped.Position - attacker.Position;
                back = Vector3.Dot(ped.ForwardVector, dir) < 0;
                quat = Quaternion.LookRotation(back ? -dir : dir);
            } else {
                back = false;
                quat = Quaternion.LookRotation(Vector3.RandomXY());
            }

            ped.Quaternion = quat;

            PedEffects.DetermineBoneSide(convertedPed.lastDamagedBone, out bool left, out bool right);
            string animName = back
                    ? "back_loop"
                    : left
                            ? "sidel_loop"
                            : right
                                    ? "sider_loop"
                                    : "front_loop";

            const AnimationFlags flags = AnimationFlags.Loop | AnimationFlags.RagdollOnCollision | AnimationFlags.AbortOnWeaponDamage;
            ped.Task.PlayAnimation("move_injured_ground", animName, 8f, -1, flags);
        }
    }
}