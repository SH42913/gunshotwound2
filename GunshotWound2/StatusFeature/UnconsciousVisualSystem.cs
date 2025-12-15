namespace GunshotWound2.StatusFeature {
    using System;
    using GTA;
    using GTA.Math;
    using GTA.NaturalMotion;
    using PedsFeature;
    using Scellecs.Morpeh;
    using TraumaFeature;
    using Utils;
    using Weighted_Randomizer;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class UnconsciousVisualSystem : ILateSystem {
        private const string CRAWL_ANIM_DICT = "move_injured_ground";

        private readonly SharedData sharedData;
        private readonly ConvertedPed.AfterRagdollAction writheAction;

        public EcsWorld World { get; set; }
        private Filter requests;

        public UnconsciousVisualSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            writheAction = StartWrithe;
        }

        public void OnAwake() {
            requests = World.Filter.With<UnconsciousVisualRequest>().With<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in requests) {
                SelectVisualBehaviour(entity, ref entity.GetComponent<ConvertedPed>());
                entity.RemoveComponent<UnconsciousVisualRequest>();
            }
        }

        void IDisposable.Dispose() { }

        private void SelectVisualBehaviour(EcsEntity entity, ref ConvertedPed convertedPed) {
            if (convertedPed.hasSpineDamage) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due Spine Damage");
#endif
                return;
            }

            if (convertedPed.requestedNmHelper != null) {
#if DEBUG
                sharedData.logger.WriteInfo("No visual behaviour due NM helper requested");
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
            convertedPed.requestedNmHelper = GetInjuredOnGroundHelper(convertedPed, leftSide, rightSide);
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
            // convertedPed.forcedAnimation = (CRAWL_ANIM_DICT, animName);
            convertedPed.RequestPermanentRagdoll();

            convertedPed.requestedNmHelper = sharedData.random.IsTrueWithProbability(0.5f)
                    ? GetInjuredOnGroundHelper(convertedPed, dontReachWithLeft: left, dontReachWithRight: right)
                    : null;
        }

        private CustomHelper GetInjuredOnGroundHelper(in ConvertedPed convertedPed,
                                                      bool dontReachWithLeft,
                                                      bool dontReachWithRight) {
            var helper = new InjuredOnGroundHelper(convertedPed.thisPed) {
                Injury1Component = convertedPed.thisPed.Bones[convertedPed.lastDamagedBone].Index,
                NumInjuries = 1,
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