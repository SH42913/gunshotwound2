namespace GunshotWound2.HealthFeature {
    using GTA;
    using GTA.Math;
    using Scellecs.Morpeh;
    using Utils;
    using WoundFeature;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class BloodPoolFxSystem : ILateSystem {
        private readonly struct Entry {
            public readonly ParticleEffectAsset asset;
            public readonly string effectName;
            public readonly float minGrowTime;

            public Entry(string assetName, string effectName, float minGrowTime) {
                asset = new ParticleEffectAsset(assetName);
                this.effectName = effectName;
                this.minGrowTime = minGrowTime;
            }
        }

        private const float GROW_TIME_SCALE = 1f;
        private readonly SharedData sharedData;

        private Filter bleedToInit;
        private Filter activePools;

        private Stash<BloodPoolFx> bloodPoolStash;
        private Stash<WoundData> woundDataStash;

        private Entry[] bloodPoolEffects;

        public EcsWorld World { get; set; }

        public BloodPoolFxSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            bleedToInit = World.Filter.With<WoundData>().Without<BloodPoolFx>();
            activePools = World.Filter.With<BloodPoolFx>();

            bloodPoolStash = World.GetStash<BloodPoolFx>().AsDisposable();
            woundDataStash = World.GetStash<WoundData>();

            bloodPoolEffects = [
                new Entry("cut_trevor1", "cs_trev1_blood_pool", GROW_TIME_SCALE * 0.6f),
                new Entry("cut_hs4", "cut_hs4_cctv_blood_pool", GROW_TIME_SCALE * 0.175f),
                new Entry("cut_sec", "cut_sec_blood_pool", GROW_TIME_SCALE * 0.175f),
            ];

            foreach (Entry effect in bloodPoolEffects) {
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
                if (woundData.totalBleed <= 0f || woundData.totalBleed >= BleedingFxSystem.BLOOD_FOUNTAIN_THRESHOLD) {
                    continue;
                }

                ref BloodPoolFx bloodPoolFx = ref bloodPoolStash.Add(entity);
                bloodPoolFx.bloodPoolIndex = sharedData.random.Next(0, bloodPoolEffects.Length);
                UpdateTimeToNextUpdate(entity, ref bloodPoolFx);
                bloodPoolFx.timeToNextUpdate *= sharedData.random.NextFloat(1f, 2f);
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
            ref Entry bloodPool = ref bloodPoolEffects[bloodPoolFx.bloodPoolIndex];
            if (!SHVDNHelpers.UseParticleFxAsset(bloodPool.asset)) {
                return;
            }

            WoundData data = woundDataStash.Get(woundEntity);
            Vector3 localPos = data.hasHitData ? data.localHitPos : Vector3.Zero;
            Vector3 worldPos = data.damagedBone.GetOffsetPosition(localPos);
            if (GTA.World.GetGroundHeight(worldPos, out float height)) {
                worldPos.Z = height;
            }

            string effectName = bloodPool.effectName;
            bloodPoolFx.effectHandle = GTAHelpers.CreateParticleEffectAtCoord(effectName, worldPos);
            if (bloodPoolFx.effectHandle != 0) {
                float alpha = sharedData.random.NextFloat(0.9f, 1f);
                GTAHelpers.SetParticleEffectAlpha(bloodPoolFx.effectHandle, alpha);
            } else {
                sharedData.logger.WriteWarning($"Effect {effectName} was not created");
            }

            bloodPoolFx.timeToStopGrow = bloodPool.minGrowTime;
        }

        private void UpdateTimeToNextUpdate(EcsEntity woundEntity, ref BloodPoolFx bloodPoolFx) {
            ref WoundData woundData = ref woundDataStash.Get(woundEntity);

            const float k = 1.1f;
            const float epsilon = 0.01f;
            const float minInterval = 1.0f;
            const float maxInterval = 8.0f;
            float calculatedTime = k / (woundData.totalBleed + epsilon);
            bloodPoolFx.timeToNextUpdate = MathHelpers.Clamp(calculatedTime, minInterval, maxInterval);
        }

        public void Dispose() {
            foreach (Entry effect in bloodPoolEffects) {
                effect.asset.MarkAsNoLongerNeeded();
            }
        }
    }
}