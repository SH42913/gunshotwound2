namespace GunshotWound2.HealthFeature {
    using Configs;
    using GTA;
    using GTA.Math;
    using Scellecs.Morpeh;
    using Utils;
    using WoundFeature;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class BloodPoolFxSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter bleedToInit;
        private Filter activePools;

        private Stash<BloodPoolFx> bloodPoolStash;
        private Stash<WoundData> woundDataStash;

        public EcsWorld World { get; set; }

        private WoundConfig.BloodPoolData[] BloodPoolParticles => sharedData.mainConfig.woundConfig.BloodPoolParticles;

        public BloodPoolFxSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            bleedToInit = World.Filter.With<WoundData>().Without<BloodPoolFx>();
            activePools = World.Filter.With<BloodPoolFx>();

            bloodPoolStash = World.GetStash<BloodPoolFx>().AsDisposable();
            woundDataStash = World.GetStash<WoundData>();

            foreach (WoundConfig.BloodPoolData effect in BloodPoolParticles) {
                effect.asset.Request();
            }
        }

        public void OnUpdate(float deltaTime) {
            InitEffects();
            UpdateActivePools(deltaTime);
        }

        private void InitEffects() {
            foreach (EcsEntity entity in bleedToInit) {
                ref WoundData woundData = ref woundDataStash.Get(entity);
                float normalizedBleed = woundData.totalBleed / sharedData.mainConfig.woundConfig.GlobalMultipliers.bleed;
                if (normalizedBleed <= 0f) {
                    continue;
                }

                ref BloodPoolFx bloodPoolFx = ref bloodPoolStash.Add(entity);
                bloodPoolFx.bloodPoolIndex = sharedData.random.Next(0, BloodPoolParticles.Length);

                if (normalizedBleed >= BleedingFxSystem.BLOOD_FOUNTAIN_THRESHOLD) {
                    bloodPoolFx.timeToNextUpdate = 4f;
                } else {
                    UpdateTimeToNextUpdate(entity, ref bloodPoolFx);
                    bloodPoolFx.timeToNextUpdate *= sharedData.random.NextFloat(1f, 2f);
                }
            }
        }

        private void UpdateActivePools(float deltaTime) {
            foreach (EcsEntity entity in activePools) {
                ref BloodPoolFx bloodPoolFx = ref bloodPoolStash.Get(entity);
                if (bloodPoolFx.effectHandle == 0) {
                    bloodPoolFx.timeToNextUpdate -= deltaTime;
                    if (bloodPoolFx.timeToNextUpdate <= 0f) {
                        CreateBloodPool(entity, ref bloodPoolFx);
                    }
                } else {
                    bloodPoolFx.timeToStopGrow -= deltaTime;
                    if (bloodPoolFx.timeToStopGrow <= 0f) {
                        GTAHelpers.RemoveParticleEffect(bloodPoolFx.effectHandle);
                        bloodPoolFx.effectHandle = 0;
                        UpdateTimeToNextUpdate(entity, ref bloodPoolFx);
                        bloodPoolFx.timeToNextUpdate *= sharedData.random.NextFloat(0.75f, 1.25f);
                    }
                }
            }
        }

        private void CreateBloodPool(EcsEntity woundEntity, ref BloodPoolFx bloodPoolFx) {
            ref WoundConfig.BloodPoolData bloodPool = ref BloodPoolParticles[bloodPoolFx.bloodPoolIndex];
            if (!SHVDNHelpers.UseParticleFxAsset(bloodPool.asset)) {
                return;
            }

            ref WoundData data = ref woundDataStash.Get(woundEntity);
            Vector3 localPos = data.hasHitData ? data.localHitPos : Vector3.Zero;
            Vector3 worldPos = data.damagedBone.GetOffsetPosition(localPos);
            if (GTA.World.GetGroundHeight(worldPos, out float height)) {
                worldPos.Z = height;
            }

            string effectName = bloodPool.effectName;
            bloodPoolFx.effectHandle = GTAHelpers.CreateParticleEffectAtCoord(effectName, worldPos);
            if (bloodPoolFx.effectHandle != 0) {
                float alpha = sharedData.random.NextFloat(0.95f, 1f);
                GTAHelpers.SetParticleEffectAlpha(bloodPoolFx.effectHandle, alpha);
            }
#if DEBUG
            else {
                sharedData.logger.WriteWarning($"Effect {effectName} was not created");
            }
#endif

            bloodPoolFx.timeToStopGrow = sharedData.mainConfig.woundConfig.BloodPoolGrowTimeScale * bloodPool.growTime;
        }

        private void UpdateTimeToNextUpdate(EcsEntity woundEntity, ref BloodPoolFx bloodPoolFx) {
            ref WoundData woundData = ref woundDataStash.Get(woundEntity);

            const float k = 1.1f;
            const float epsilon = 0.01f;
            const float minInterval = 1.0f;
            const float maxInterval = 8.0f;
            float calculatedTime = k / (woundData.totalBleed + epsilon);
            bloodPoolFx.timeToNextUpdate = MathHelpers.Clamp(calculatedTime, minInterval, maxInterval);

            Ped ped = woundData.damagedBone.Owner;
            if (ped.IsRunning) {
                bloodPoolFx.timeToStopGrow *= 8f;
                bloodPoolFx.timeToNextUpdate *= sharedData.random.NextFloat(0.6f, 0.9f);
            } else if (ped.IsSprinting) {
                bloodPoolFx.timeToStopGrow *= 8f;
                bloodPoolFx.timeToNextUpdate *= sharedData.random.NextFloat(0.3f, 0.6f);
            }
        }

        public void Dispose() {
            foreach (WoundConfig.BloodPoolData effect in BloodPoolParticles) {
                effect.asset.MarkAsNoLongerNeeded();
            }
        }
    }
}