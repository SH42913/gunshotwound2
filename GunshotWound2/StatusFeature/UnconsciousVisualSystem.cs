namespace GunshotWound2.StatusFeature {
    using System;
    using GTA;
    using GTA.Math;
    using GTA.NaturalMotion;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using TraumaFeature;
    using Utils;
    using Weighted_Randomizer;
    using WoundFeature;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class UnconsciousVisualSystem : ILateSystem {
        private const string CRAWL_ANIM_DICT = "move_injured_ground";

        private readonly SharedData sharedData;
        private readonly ConvertedPed.AfterRagdollAction writheAction;

        public EcsWorld World { get; set; }
        private Filter requests;

        private Stash<Bleeding> bleedingStash;
        private Stash<WoundData> woundDataStash;

        public UnconsciousVisualSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            writheAction = StartWrithe;
        }

        public void OnAwake() {
            requests = World.Filter.With<UnconsciousVisualRequest>().With<ConvertedPed>();

            bleedingStash = World.GetStash<Bleeding>();
            woundDataStash = World.GetStash<WoundData>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in requests) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                SelectVisualBehaviour(entity, ref convertedPed);
                entity.RemoveComponent<UnconsciousVisualRequest>();
            }
        }

        void IDisposable.Dispose() { }

        private void SelectVisualBehaviour(EcsEntity entity, ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Selecting unconscious visual behaviour");
#endif
            if (convertedPed.naturalMotionBuilder != null) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due NM helper requested");
#endif
                return;
            }

            if (convertedPed.hasSpineDamage) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due Spine Damage");
#endif
                return;
            }

            if (!sharedData.mainConfig.woundConfig.UseCustomUnconsciousBehaviour) {
#if DEBUG
                sharedData.logger.WriteInfo("Visual behaviour is simple ragdoll");
#endif
                convertedPed.RequestPermanentRagdoll();
                return;
            }

            IWeightedRandomizer<int> randomizer = sharedData.weightRandom;
            randomizer.Clear();
            randomizer.Add(0);
            randomizer.Add(1, weight: 3);
            randomizer.Add(4);

            if (!convertedPed.thisPed.IsInVehicle()) {
                // TODO: Restore later
                // ref Traumas traumas = ref entity.GetComponent<Traumas>(out bool hasTraumas);
                // bool majorInjury = hasTraumas
                //                    && (traumas.HasActive(Traumas.Effects.Heart)
                //                        || traumas.HasActive(Traumas.Effects.Lungs)
                //                        || traumas.HasActive(Traumas.Effects.Abdomen));

                // bool legsDamaged = traumas.HasActive(Traumas.Effects.Legs);
                // if (majorInjury || legsDamaged) {
                //     randomizer.Add(2);
                // }

                // float totalSeverity = HealthFeature.HealthFeature.CalculateSeverityOfAllBleedingWounds(entity);
                // float timeToDeath = convertedPed.CalculateTimeToDeath(totalSeverity);
                // if (majorInjury && timeToDeath <= 30f && !convertedPed.isPlayer) {
                //     randomizer.Add(3);
                // }
            }

            switch (randomizer.NextWithReplacement()) {
                case 0:  BodyRelaxRagdollVisualBehaviour(ref convertedPed); break;
                case 1:  InjuredVisualBehaviour(ref convertedPed); break;
                case 2:  CrawlVisualBehaviour(entity, ref convertedPed); break;
                case 3:  WritheVisualBehaviour(ref convertedPed); break;
                case 4:  BodyWritheVisualBehaviour(ref convertedPed); break;
                default: throw new Exception("Incorrect visual behaviour index");
            }
        }

        private void BodyRelaxRagdollVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Body relax as visual behaviour");
#endif
            convertedPed.thisPed.StopCurrentPlayingSpeech();
            convertedPed.thisPed.StopCurrentPlayingAmbientSpeech();
            convertedPed.thisPed.IsPainAudioEnabled = false;

            convertedPed.naturalMotionBuilder = static (_, _, ped) => new BodyRelaxHelper(ped);
            convertedPed.RequestPermanentRagdoll();
        }

        private void BodyWritheVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("BodyWritheHelper as visual behaviour");
#endif

            // ReSharper disable once ParameterHidesMember
            convertedPed.SetNaturalMotionBuilder((sharedData, entity, ped) => {
                CheckArmTraumas(entity, out bool leftArmBroken, out bool rightArmBroken);
                CheckLegTraumas(entity, out bool leftLegBroken, out bool rightLegBroken);
                return new BodyWritheHelper(ped) {
                    ApplyStiffness = true,

                    ArmStiffness = sharedData.random.NextFloat(5f, 8f),
                    BackStiffness = sharedData.random.NextFloat(5f, 8f),
                    LegStiffness = sharedData.random.NextFloat(5f, 8f),

                    ArmPeriod = sharedData.random.NextFloat(2.5f, 3.5f),
                    BackPeriod = sharedData.random.NextFloat(3.0f, 4.0f),
                    LegPeriod = sharedData.random.NextFloat(2.5f, 3.5f),

                    ArmAmplitude = 0.5f,
                    BackAmplitude = 0.3f,
                    LegAmplitude = 0.4f,

                    ArmDamping = sharedData.random.NextFloat(1f, 1.5f),
                    BackDamping = sharedData.random.NextFloat(1f, 1.5f),
                    LegDamping = sharedData.random.NextFloat(1f, 1.5f),

                    BlendArms = leftArmBroken || rightArmBroken ? 0.2f : 0.4f,
                    BlendLegs = leftLegBroken || rightLegBroken ? 0.2f : 0.8f,
                };
            });

            convertedPed.RequestPermanentRagdoll();
        }

        private void WritheVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Default writhe as visual behaviour");
#endif
            convertedPed.RequestRagdoll(3000);
            convertedPed.afterRagdollAction = writheAction;
        }

        private void CrawlVisualBehaviour(EcsEntity entity, ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("Crawl animation as visual behaviour");
#endif
            StartCrawl(entity, ref convertedPed);
        }

        private void InjuredVisualBehaviour(ref ConvertedPed convertedPed) {
#if DEBUG
            sharedData.logger.WriteInfo("InjuredOnGroundHelper as visual behaviour");
#endif
            convertedPed.SetNaturalMotionBuilder(BuildInjuredOnGroundNM());
            convertedPed.RequestPermanentRagdoll();
        }

        private void StartWrithe(ref ConvertedPed convertedPed) {
            convertedPed.forceRemove = true;

            Ped ped = convertedPed.thisPed;
            ped.Task.ClearAllImmediately();
            ped.CanWrithe = true;
            ped.BlockPermanentEvents = true;

            int loops = sharedData.random.Next(1, 3);
            PedEffects.StartWritheTask(ped, loops, convertedPed.lastAggressor);
        }

        private void StartCrawl(EcsEntity entity, ref ConvertedPed convertedPed) {
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

            CheckArmTraumas(entity, out bool leftArmBroken, out bool rightArmBroken);
            string animName = back
                    ? "back_loop"
                    : leftArmBroken
                            ? "sidel_loop"
                            : rightArmBroken
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

            // convertedPed.forcedAnimation = (CRAWL_ANIM_DICT, animName);
            convertedPed.RequestPermanentRagdoll();
            if (sharedData.random.IsTrueWithProbability(0.5f)) {
                convertedPed.SetNaturalMotionBuilder(BuildInjuredOnGroundNM());
            }
        }

        private ConvertedPed.NaturalMotionBuilder BuildInjuredOnGroundNM() {
            return (_, ent, ped) => GetInjuredOnGroundHelper(ent, ped);
        }

        private CustomHelper GetInjuredOnGroundHelper(EcsEntity entity, Ped ped) {
            CheckArmTraumas(entity, out bool dontReachWithLeft, out bool dontReachWithRight);

            var helper = new InjuredOnGroundHelper(ped) {
                DontReachWithLeft = dontReachWithLeft,
                DontReachWithRight = dontReachWithRight,
            };

            int numInjuries = 0;
            (WoundData? wound1, WoundData? wound2) = SelectTopWounds(entity);
            if (wound1.HasValue) {
                WoundData data = wound1.Value;
                var nmData = NMHelper.GetNaturalMotionData(data.damagedBone, data.localHitPos, data.localHitNormal);
                if (nmData.nmIndex != 0) {
                    helper.Injury1Component = nmData.nmIndex;
                    helper.Injury1LocalPosition = nmData.localNmPos;
                    helper.Injury1LocalNormal = nmData.localNmNormal;
                    numInjuries++;
                }
            }

            if (wound2.HasValue) {
                WoundData data = wound2.Value;
                var nmData = NMHelper.GetNaturalMotionData(data.damagedBone, data.localHitPos, data.localHitNormal);
                if (nmData.nmIndex != 0) {
                    helper.Injury2Component = nmData.nmIndex;
                    helper.Injury2LocalPosition = nmData.localNmPos;
                    helper.Injury2LocalNormal = nmData.localNmNormal;
                    numInjuries++;
                }
            }

            ref Pain pain = ref entity.GetComponent<Pain>();
            ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
            helper.StrongRollForce = pain.Percent() < 3.0f;
            helper.NumInjuries = numInjuries;
            helper.AttackerPos = convertedPed.lastAggressor.IsValid()
                    ? convertedPed.lastAggressor.Position
                    : convertedPed.thisPed.Position;

            return helper;
        }

        private (WoundData? wound1, WoundData? wound2) SelectTopWounds(EcsEntity entity) {
            (WoundData? wound1, WoundData? wound2) = (null, null);
            float wound1Score = -1f;
            float wound2Score = -1f;

            ref Health health = ref entity.GetComponent<Health>();
            if (health.bleedingWounds == null || health.bleedingWounds.Count < 1) {
                return default;
            }

            foreach (EcsEntity woundEntity in health.bleedingWounds) {
                ref WoundData woundData = ref woundDataStash.Get(woundEntity, out bool hasData);
                if (!hasData) {
                    continue;
                }

                float currentScore = CalculateScore(woundData);
                if (currentScore > wound1Score) {
                    wound2 = wound1;
                    wound2Score = wound1Score;

                    wound1 = woundData;
                    wound1Score = currentScore;
                } else if (currentScore > wound2Score) {
                    wound2 = woundData;
                    wound2Score = currentScore;
                }
            }

            return (wound1, wound2);

            static float CalculateScore(in WoundData data) => data.totalPain + (data.totalBleed * 50f);
        }

        private void CheckArmTraumas(EcsEntity entity, out bool leftArmBroken, out bool rightArmBroken) {
            ref Traumas traumas = ref entity.GetComponent<Traumas>(out bool hasTraumas);
            if (!hasTraumas || !traumas.HasActive(Traumas.Effects.Arms)) {
                leftArmBroken = false;
                rightArmBroken = false;
                return;
            }

            leftArmBroken = false;
            rightArmBroken = false;
            foreach (EcsEntity traumaEntity in traumas.traumas) {
                ref Bleeding bleeding = ref bleedingStash.Get(traumaEntity);
                leftArmBroken |= bleeding.bodyPart.IsLeftArm();
                rightArmBroken |= bleeding.bodyPart.IsRightArm();
            }
        }
    }
}