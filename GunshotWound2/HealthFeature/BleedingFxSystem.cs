#define DEBUG_EVERY_FRAME
namespace GunshotWound2.HealthFeature {
    using GTA;
    using GTA.Math;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using WoundFeature;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class BleedingFxSystem : ILateSystem {
        public const float BLOOD_FOUNTAIN_THRESHOLD = 0.55f;

        // ReSharper disable once NotAccessedField.Local
        private readonly SharedData sharedData;

        private Stash<Bleeding> bleedStash;
        private Stash<BleedingFx> bleedFxStash;

        private Filter bleedToInit;
        private Filter activeParticles;
        private Filter particlesToClean;

        private ParticleEffectAsset coreAsset;
        private ParticleEffectAsset cutMichael2Asset;
        private ParticleEffectAsset cutSecAsset;

        public EcsWorld World { get; set; }

        public BleedingFxSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            bleedStash = World.GetStash<Bleeding>();
            bleedFxStash = World.GetStash<BleedingFx>().AsDisposable();

            bleedToInit = World.Filter.With<Bleeding>().Without<BleedingFx>();
            activeParticles = World.Filter.With<BleedingFx>();
            particlesToClean = activeParticles.Without<Bleeding>();

            coreAsset = new ParticleEffectAsset("core");
            cutMichael2Asset = new ParticleEffectAsset("cut_michael2");
            cutSecAsset = new ParticleEffectAsset("cut_sec");

            coreAsset.Request();
            cutMichael2Asset.Request();
            cutSecAsset.Request();
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

                PedBone damagedBone = woundData.damagedBone;
                if (damagedBone == null || !damagedBone.IsValid) {
                    ref ConvertedPed convertedPed = ref bleeding.target.GetComponent<ConvertedPed>();
                    int randomBoneId = sharedData.random.NextFromCollection(bleeding.bodyPart.Bones);
                    int boneIndex = PedEffects.GetBoneIndex(convertedPed.thisPed, randomBoneId);
                    damagedBone = convertedPed.thisPed.Bones[boneIndex];
#if DEBUG
                    sharedData.logger.WriteInfo($"Used random FX data for {bleeding.name}, boneId:{randomBoneId}");
#endif
                }

                if (!hasWoundData || !TryGetFromWoundData(woundData, out Vector3 localHitPos, out Vector3 localHitNormal)) {
                    localHitPos = Vector3.Zero;
                    localHitNormal = Vector3.RandomXY();
                }

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
                GetEffectForBluntBleeding(severity, out asset, out effectName);
            } else {
                GetEffectForPenetrationBleeding(severity, out asset, out effectName);
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

        private void GetEffectForPenetrationBleeding(float severity, out ParticleEffectAsset asset, out string effectName) {
            switch (severity) {
                case > BLOOD_FOUNTAIN_THRESHOLD:
                    asset = cutMichael2Asset;
                    effectName = "cs_mich2_blood_head_leak";
                    break;
                case > 0.2f:
                    asset = coreAsset;
                    effectName = "ped_blood_drips";
                    break;
                case > 0.01f:
                    asset = cutSecAsset;
                    effectName = "cut_sec_golfclub_blood_drips";
                    break;
                default:
                    asset = default;
                    effectName = null;
                    break;
            }
        }

        private void GetEffectForBluntBleeding(float severity, out ParticleEffectAsset asset, out string effectName) {
            switch (severity) {
                case > 0.04f:
                    asset = coreAsset;
                    effectName = "ped_blood_drips";
                    break;
                case > 0.01f:
                    asset = cutSecAsset;
                    effectName = "cut_sec_golfclub_blood_drips";
                    break;
                default:
                    asset = default;
                    effectName = null;
                    break;
            }
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

            coreAsset.MarkAsNoLongerNeeded();
            cutMichael2Asset.MarkAsNoLongerNeeded();
            cutSecAsset.MarkAsNoLongerNeeded();
        }
    }
}