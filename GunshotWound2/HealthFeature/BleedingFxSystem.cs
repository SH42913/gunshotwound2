#define DEBUG_EVERY_FRAME
namespace GunshotWound2.HealthFeature {
    using Configs;
    using GTA;
    using GTA.Math;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using WoundFeature;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class BleedingFxSystem : ILateSystem {
        // ReSharper disable once NotAccessedField.Local
        private readonly SharedData sharedData;

        private Stash<Bleeding> bleedStash;
        private Stash<BleedingFx> bleedFxStash;

        private Filter bleedToInit;
        private Filter activeParticles;
        private Filter particlesToClean;

        public EcsWorld World { get; set; }

        private WoundConfig.BleedingFxData[] PenetratingEffects => sharedData.mainConfig.woundConfig.PenetratingBleedingEffects;
        private WoundConfig.BleedingFxData[] BluntEffects => sharedData.mainConfig.woundConfig.BluntBleedingEffects;

        public BleedingFxSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            bleedStash = World.GetStash<Bleeding>();
            bleedFxStash = World.GetStash<BleedingFx>().AsDisposable();

            bleedToInit = World.Filter.With<Bleeding>().Without<BleedingFx>();
            activeParticles = World.Filter.With<BleedingFx>();
            particlesToClean = activeParticles.Without<Bleeding>();

            RequestAssets(PenetratingEffects);
            RequestAssets(BluntEffects);
        }

        public void OnUpdate(float deltaTime) {
            InitFx();
            CleanFx();
        }

        private void InitFx() {
            foreach (EcsEntity entity in bleedToInit) {
                ref Bleeding bleeding = ref bleedStash.Get(entity);
                if (bleeding.isTrauma) {
                    continue;
                }

                ref WoundData woundData = ref entity.GetComponent<WoundData>(out bool hasWoundData);
#if DEBUG
                if (!hasWoundData) {
                    sharedData.logger.WriteInfo($"No {nameof(woundData)} at {entity.ID}");
                }
#endif

                if (!hasWoundData || !TryGetFromWoundData(woundData, out Vector3 localHitPos, out Vector3 localHitNormal)) {
                    localHitPos = Vector3.Zero;
                    localHitNormal = Vector3.RandomXY();
                }

                PedBone damagedBone = woundData.damagedBone;
                Quaternion boneQuaternion = damagedBone.Quaternion;
                Quaternion boneQuaternionInv = Quaternion.Invert(boneQuaternion);
                Quaternion fromToQuaternion = boneQuaternion * Quaternion.LookRotation(boneQuaternionInv * localHitNormal);
                Vector3 localRotation = fromToQuaternion.ToEuler();

                float severity = hasWoundData ? woundData.totalBleed : bleeding.severity;
                bleedFxStash.Add(entity).particles = SpawnParticles(severity, bleeding, damagedBone, localHitPos, localRotation);
            }
        }

        private bool TryGetFromWoundData(in WoundData woundData,
                                         out Vector3 localHitPos,
                                         out Vector3 localHitNormal) {
            localHitPos = default;
            localHitNormal = default;

            if (!woundData.hasHitData) {
#if DEBUG
                sharedData.logger.WriteInfo($"{nameof(WoundData)} doesn't have hit data");
#endif
                return false;
            }

            localHitPos = woundData.localHitPos;
            localHitNormal = woundData.localHitNormal;
            return true;
        }

        private void CleanFx() {
            foreach (EcsEntity entity in particlesToClean) {
                CleanEntity(entity);
            }
        }

        private ParticleEffect SpawnParticles(float severity,
                                              in Bleeding bleeding,
                                              PedBone targetBone,
                                              Vector3 localHitPos,
                                              Vector3 localRot) {
            severity /= sharedData.mainConfig.woundConfig.GlobalMultipliers.bleed;

            ParticleEffectAsset asset;
            string effectName;
            if (bleeding.bluntDamageReason) {
                GetEffectBySeverity(BluntEffects, severity, out asset, out effectName);
            } else {
                GetEffectBySeverity(PenetratingEffects, severity, out asset, out effectName);
            }

            if (string.IsNullOrEmpty(effectName)) {
                return null;
            }

#if DEBUG && DEBUG_EVERY_FRAME
            var message = $"Spawn particle {asset.AssetName}:{effectName} at {targetBone.Owner.Handle}({targetBone.Name})";
            sharedData.logger.WriteInfo(message);
            sharedData.logger.WriteInfo($"{bleeding.name}, normalizedSeverity:{severity} LocalPos:{localHitPos} LocalRot:{localRot}");
#endif

            return GTA.World.CreateParticleEffect(asset, effectName, targetBone, localHitPos, localRot);
        }

        private static void GetEffectBySeverity(WoundConfig.BleedingFxData[] effects,
                                                float severity,
                                                out ParticleEffectAsset asset,
                                                out string effectName) {
            for (int i = 0; i < effects.Length; i++) {
                ref WoundConfig.BleedingFxData fx = ref effects[i];
                if (severity > fx.severity) {
                    asset = fx.asset;
                    effectName = fx.effectName;
                    return;
                }
            }

            asset = default;
            effectName = null;
        }

        private void CleanEntity(EcsEntity entity) {
            ref BleedingFx bleedingFx = ref bleedFxStash.Get(entity);
            if (bleedingFx.particles != null) {
                bleedingFx.particles.Delete();
            }

            bleedFxStash.Remove(entity);
        }

        public void Dispose() {
            foreach (EcsEntity entity in activeParticles) {
                CleanEntity(entity);
            }

            DisposeAssets(PenetratingEffects);
            DisposeAssets(BluntEffects);
        }

        private static void RequestAssets(WoundConfig.BleedingFxData[] effects) {
            foreach (WoundConfig.BleedingFxData effect in effects) {
                effect.asset.Request();
            }
        }

        private static void DisposeAssets(WoundConfig.BleedingFxData[] effects) {
            foreach (WoundConfig.BleedingFxData effect in effects) {
                effect.asset.MarkAsNoLongerNeeded();
            }
        }
    }
}