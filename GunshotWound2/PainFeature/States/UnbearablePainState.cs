namespace GunshotWound2.PainFeature.States {
    using System;
    using GTA;
    using GTA.Math;
    using GTA.NaturalMotion;
    using PedsFeature;
    using HealthFeature;
    using Scellecs.Morpeh;
    using TraumaFeature;
    using Utils;
    using Weighted_Randomizer;

    public sealed class UnbearablePainState : IPainState {
        public const float PAIN_THRESHOLD = 1f;
        private const string CRAWL_ANIM_DICT = "move_injured_ground";

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

        private readonly SharedData sharedData;

        public UnbearablePainState(SharedData sharedData) {
            this.sharedData = sharedData;

            writheAction = StartWrithe;
        }

        public void ApplyPainIncreased(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.Percent() - PainThreshold < 0.1f) {
                pain.diff += 0.2f * pain.max;
            }

            convertedPed.isRestrictToDrive = true;
            SelectVisualBehaviour(pedEntity, ref convertedPed);

            int deathAnimIndex = sharedData.random.Next(1, 3);
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"die_{deathAnimIndex.ToString()}", convertedPed.isMale);
            convertedPed.thisPed.StopCurrentPlayingSpeech();

            if (convertedPed.isPlayer) {
                PlayerOnlyCase(ref convertedPed);
            } else {
                NonPlayerCase(ref convertedPed);
            }
        }

        public void ApplyPainDecreased(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.isRestrictToDrive = false;

            PedEffects.StopAnimation(convertedPed.thisPed, convertedPed.forcedAnimation);
            convertedPed.forcedAnimation = default;

            if (convertedPed.isPlayer) {
                SetPlayerIsIgnoredByPeds(Game.Player, false);
                sharedData.cameraService.SetUnconsciousEffect(false);
            }
        }

        public bool TryGetMoveSets(in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = null;
            return false;
        }

        public bool TryGetMoodSets(in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }

        private void PlayerOnlyCase(ref ConvertedPed convertedPed) {
            string speech = sharedData.random.Next(PLAYER_DEATH_AMBIENT);
            convertedPed.thisPed.PlayAmbientSpeech(speech, SpeechModifier.InterruptShouted);

            Player player = Game.Player;
            if (player.Wanted.WantedLevel <= 2) {
                SetPlayerIsIgnoredByPeds(Game.Player, true);
                if (sharedData.mainConfig.playerConfig.PoliceCanForgetYou) {
                    const bool delayLawResponse = false;
                    player.Wanted.SetWantedLevel(0, delayLawResponse);
                    player.Wanted.ApplyWantedLevelChangeNow(delayLawResponse);
                }
            }

            if (sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            if (!convertedPed.hasSpineDamage) {
                sharedData.notifier.critical.QueueMessage(sharedData.localeConfig.UnbearablePainMessage);
            }

            sharedData.cameraService.SetUnconsciousEffect(true);
        }

        private void NonPlayerCase(ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
            ped.Weapons.Drop();

            string speech = sharedData.random.Next(NON_PLAYER_DEATH_AMBIENT);
            ped.PlayAmbientSpeech(speech, SpeechModifier.InterruptShouted);

            bool isInVehicle = ped.IsInVehicle();
            bool notDriver = ped.SeatIndex != VehicleSeat.Driver;
            if (isInVehicle && notDriver && sharedData.random.IsTrueWithProbability(0.5f)) {
                ped.Task.LeaveVehicle(LeaveVehicleFlags.BailOut);
            }
        }

        private void SetPlayerIsIgnoredByPeds(Player player, bool value) {
            if (sharedData.mainConfig.playerConfig.PedsWillIgnoreUnconsciousPlayer) {
                player.Wanted.SetEveryoneIgnorePlayer(value);
            }
        }

        private void SelectVisualBehaviour(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.hasSpineDamage) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due Spine Damage");
#endif
                return;
            }

            if (!sharedData.mainConfig.woundConfig.UseCustomUnconsciousBehaviour) {
                return;
            }

            IWeightedRandomizer<int> randomizer = sharedData.weightRandom;
            randomizer.Clear();
            randomizer.Add(0);
            randomizer.Add(1, weight: 2);

            if (!convertedPed.thisPed.IsInVehicle()) {
                ref Traumas traumas = ref pedEntity.GetComponent<Traumas>();
                bool legsDamaged = traumas.HasActive(Traumas.Effects.LegsCrit);
                bool heavyCrit = traumas.HasActive(Traumas.Effects.HeartCrit)
                                 || traumas.HasActive(Traumas.Effects.LungsCrit)
                                 || traumas.HasActive(Traumas.Effects.AbdomenCrit);

                if (heavyCrit || legsDamaged) {
                    randomizer.Add(2);
                }

                float totalSeverity = HealthFeature.CalculateSeverityOfAllBleedingWounds(pedEntity);
                float timeToDeath = convertedPed.CalculateTimeToDeath(totalSeverity);
                if (heavyCrit && timeToDeath <= 30f && !convertedPed.isPlayer) {
                    randomizer.Add(3);
                }
            }

            switch (randomizer.NextWithReplacement()) {
                case 0:  SimpleRagdollVisualBehaviour(ref convertedPed); break;
                case 1:  InjuredVisualBehaviour(ref convertedPed); break;
                case 2:  CrawlVisualBehaviour(ref convertedPed); break;
                case 3:  WritheVisualBehaviour(ref convertedPed); break;
                default: throw new Exception("Incorrect visual behaviour index");
            }
        }

        private void SimpleRagdollVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Simple ragdoll as visual behaviour");
#endif
            convertedPed.RequestPermanentRagdoll();
        }

        private void WritheVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Default writhe as visual behaviour");
#endif
            convertedPed.RequestRagdoll(3000);
            convertedPed.afterRagdollAction = writheAction;
        }

        private void CrawlVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Crawl animation as visual behaviour");
#endif
            StartCrawl(ref convertedPed);
        }

        private void InjuredVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("InjuredOnGroundHelper as visual behaviour");
#endif
            PedEffects.DetermineBoneSide(convertedPed.lastDamagedBone, out bool leftSide, out bool rightSide);
            convertedPed.RequestPermanentRagdoll();
            convertedPed.nmHelper = GetInjuredOnGroundHelper(convertedPed, leftSide, rightSide);
        }

        private void StartWrithe(ref ConvertedPed convertedPed) {
            convertedPed.forceRemove = true;

            Ped ped = convertedPed.thisPed;
            ped.Task.ClearAllImmediately();
            ped.CanWrithe = true;
            ped.BlockPermanentEvents = true;

            int loops = sharedData.random.Next(1, 5);
            PedEffects.StartWritheTask(ped, loops, convertedPed.lastAggressor);
        }

        private void StartCrawl(ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
            ped.BlockPermanentEvents = true;

            bool back;
            Quaternion quat;
            Vector3 pedPosition = ped.Position;
            if (convertedPed.lastAggressor.IsValid()) {
                Vector3 dir = pedPosition - convertedPed.lastAggressor.Position;
                back = Vector3.Dot(ped.ForwardVector, dir) < 0;
                quat = Quaternion.LookRotation(back ? -dir : dir);
            } else {
                back = false;
                quat = Quaternion.LookRotation(Vector3.RandomXY());
            }

            PedEffects.DetermineBoneSide(convertedPed.lastDamagedBone, out bool left, out bool right);
            string animName = back
                    ? "back_loop"
                    : left
                            ? "sidel_loop"
                            : right
                                    ? "sider_loop"
                                    : "front_loop";

            const AnimationFlags flags = AnimationFlags.Loop | AnimationFlags.AbortOnWeaponDamage;
            int duration = sharedData.random.Next(5000, 30000);

            ped.Task.ClearAllImmediately();
            ped.Task.PlayAnimationAdvanced(new CrClipAsset(CRAWL_ANIM_DICT, animName),
                                           pedPosition,
                                           quat.ToEuler(),
                                           AnimationBlendDelta.SlowBlendIn,
                                           AnimationBlendDelta.InstantBlendOut,
                                           timeToPlay: duration,
                                           flags);

#if DEBUG
            sharedData.logger.WriteInfo($"Selected animation = {animName} for {duration} ms");
#endif
            convertedPed.forcedAnimation = (CRAWL_ANIM_DICT, animName);
            convertedPed.RequestPermanentRagdoll();

            convertedPed.nmHelper = sharedData.random.IsTrueWithProbability(0.5f)
                    ? GetInjuredOnGroundHelper(convertedPed, dontReachWithLeft: left, dontReachWithRight: right)
                    : null;
        }

        private InjuredOnGroundHelper GetInjuredOnGroundHelper(in ConvertedPed convertedPed,
                                                               bool dontReachWithLeft,
                                                               bool dontReachWithRight) {
            var helper = new InjuredOnGroundHelper(convertedPed.thisPed) {
                Injury1Component = (int)convertedPed.lastDamagedBone,
                NumInjuries = sharedData.random.Next(1, 3),
                DontReachWithLeft = dontReachWithLeft,
                DontReachWithRight = dontReachWithRight,
                StrongRollForce = sharedData.random.IsTrueWithProbability(0.5f),
            };

            if (convertedPed.lastAggressor.IsValid()) {
                helper.AttackerPos = convertedPed.lastAggressor.Position;
            }

            return helper;
        }
    }
}