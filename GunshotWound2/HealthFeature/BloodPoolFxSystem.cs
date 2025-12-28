namespace GunshotWound2.HealthFeature {
    using GTA;
    using GTA.Math;
    using PedsFeature;
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

        private Filter peds;
        private Stash<Health> healthStash;
        private Stash<ConvertedPed> pedStash;
        private Stash<BloodPoolFx> bloodPoolStash;
        private Stash<WoundData> woundDataStash;

        private Entry[] bloodPoolEffects;

        public EcsWorld World { get; set; }

        public BloodPoolFxSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Health>();
            healthStash = World.GetStash<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            bloodPoolStash = World.GetStash<BloodPoolFx>().AsDisposable();
            woundDataStash = World.GetStash<WoundData>();

            bloodPoolEffects = [
                new Entry("cut_trevor1", "cs_trev1_blood_pool", GROW_TIME_SCALE * 0.65f),
                new Entry("cut_hs4", "cut_hs4_cctv_blood_pool", GROW_TIME_SCALE * 0.175f),
                new Entry("cut_sec", "cut_sec_blood_pool", GROW_TIME_SCALE * 0.175f),
            ];

            foreach (Entry effect in bloodPoolEffects) {
                effect.asset.Request();
            }
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (health.isDead) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                bool canCreateBloodPool = convertedPed.isRagdoll && health.HasBleedingWounds();
                if (bloodPoolStash.Has(entity)) {
                    if (!canCreateBloodPool) {
                        bloodPoolStash.Remove(entity);
                    }
                } else {
                    if (canCreateBloodPool) {
                        bloodPoolStash.Set(entity, new BloodPoolFx {
                            startDelay = 2f,
                            bloodPoolIndex = sharedData.random.Next(0, bloodPoolEffects.Length)
                        });
                    }
                }

                if (!canCreateBloodPool) {
                    continue;
                }

                ref BloodPoolFx bloodPoolFx = ref bloodPoolStash.Get(entity);
                if (bloodPoolFx.startDelay > 0f) {
                    bloodPoolFx.startDelay -= deltaTime;
                    continue;
                }

                if (bloodPoolFx.effectHandle == 0) {
                    bloodPoolFx.timeToNextUpdate -= deltaTime;
                    if (bloodPoolFx.timeToNextUpdate <= 0f) {
                        RefreshMostBleedingWound(ref bloodPoolFx, health);
                        CreateBloodPool(ref bloodPoolFx);
                    }
                } else {
                    bloodPoolFx.timeToStopGrow -= deltaTime;
                    if (bloodPoolFx.timeToStopGrow <= 0f) {
                        GTAHelpers.RemoveParticleEffect(bloodPoolFx.effectHandle);
                        bloodPoolFx.effectHandle = 0;
                        UpdateTimeToNextUpdate(ref bloodPoolFx);
                    }
                }
            }
        }

        private void RefreshMostBleedingWound(ref BloodPoolFx bloodPoolFx, in Health health) {
            WoundData mostBleedingWoundData = default;
            EcsEntity mostBleedingWoundEntity = null;
            foreach (EcsEntity entity in health.bleedingWounds) {
                ref WoundData woundData = ref woundDataStash.Get(entity, out bool hasData);
                if (!hasData) {
                    continue;
                }

                bool skipWound = woundData.totalBleed > BleedingFxSystem.BLOOD_FOUNTAIN_THRESHOLD;
                if (woundData.totalBleed > mostBleedingWoundData.totalBleed && !skipWound) {
                    mostBleedingWoundData = woundData;
                    mostBleedingWoundEntity = entity;
                }
            }

            bloodPoolFx.mostBleedingWound = mostBleedingWoundEntity.IsNullOrDisposed() ? null : mostBleedingWoundEntity;
        }

        private void CreateBloodPool(ref BloodPoolFx bloodPoolFx) {
            if (bloodPoolFx.mostBleedingWound.IsNullOrDisposed()) {
                return;
            }

            ref Entry bloodPool = ref bloodPoolEffects[bloodPoolFx.bloodPoolIndex];
            if (!SHVDNHelpers.UseParticleFxAsset(bloodPool.asset)) {
                return;
            }

            WoundData data = woundDataStash.Get(bloodPoolFx.mostBleedingWound);
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

        private void UpdateTimeToNextUpdate(ref BloodPoolFx bloodPoolFx) {
            if (bloodPoolFx.mostBleedingWound.IsNullOrDisposed()) {
                bloodPoolFx.timeToNextUpdate = 1f;
                return;
            }

            ref WoundData woundData = ref woundDataStash.Get(bloodPoolFx.mostBleedingWound, out bool hasData);
            if (!hasData) {
                bloodPoolFx.timeToNextUpdate = 1f;
                return;
            }

            const float k = 0.5f;
            const float epsilon = 0.01f;
            const float maxInterval = 8.0f;
            float calculatedTime = k / (woundData.totalBleed + epsilon);
            float minInterval = bloodPoolEffects[bloodPoolFx.bloodPoolIndex].minGrowTime;
            bloodPoolFx.timeToNextUpdate = MathHelpers.Clamp(calculatedTime, minInterval, maxInterval);
        }

        public void Dispose() {
            foreach (Entry effect in bloodPoolEffects) {
                effect.asset.MarkAsNoLongerNeeded();
            }
        }
    }
}