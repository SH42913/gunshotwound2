namespace GunshotWound2.HealthFeature {
    using GTA;
    using GTA.Math;
    using HitDetection;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class BleedingFxSystem : ILateSystem {
        // ReSharper disable once NotAccessedField.Local
        private readonly SharedData sharedData;

        private Stash<Bleeding> bleedStash;
        private Stash<BleedingFx> bleedFxStash;

        private Filter bleedToInit;
        private Filter activeParticles;
        private Filter bleedToClean;

        private ParticleEffectAsset coreAsset;
        private ParticleEffectAsset cutMichael2Asset;
        private ParticleEffectAsset cutSecAsset;

        public EcsWorld World { get; set; }

        public BleedingFxSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            bleedStash = World.GetStash<Bleeding>();
            bleedFxStash = World.GetStash<BleedingFx>();

            bleedToInit = World.Filter.With<Bleeding>().Without<BleedingFx>();
            activeParticles = World.Filter.With<BleedingFx>();
            bleedToClean = World.Filter.With<BleedingFx>().Without<Bleeding>();

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
                ref ConvertedPed convertedPed = ref bleeding.target.GetComponent<ConvertedPed>();
                if (!TryGetFromHitData(bleeding.target, out PedBone damagedBone, out Vector3 localHitPos, out Vector3 hitNormal)) {
                    int randomBoneId = sharedData.random.NextFromCollection(bleeding.bodyPart.Bones);
                    int boneIndex = PedEffects.GetBoneIndex(convertedPed.thisPed, randomBoneId);
                    damagedBone = convertedPed.thisPed.Bones[boneIndex];
                    localHitPos = damagedBone.Pose;
                    hitNormal = Vector3.RandomXYZ();
#if DEBUG
                    sharedData.logger.WriteInfo($"Used random FX data for {bleeding.name}, boneId:{randomBoneId}");
#endif
                }

                Quaternion fromToQuaternion = damagedBone.Quaternion * Quaternion.LookRotation(hitNormal);
                Vector3 localRotation = fromToQuaternion.ToEuler();

                ref BleedingFx bleedingFx = ref bleedFxStash.Add(entity);
                bleedingFx.particles = SpawnParticles(bleeding, damagedBone, localHitPos, localRotation);
            }
        }

        private bool TryGetFromHitData(EcsEntity entity,
                                       out PedBone damagedBone,
                                       out Vector3 localHitPos,
                                       out Vector3 hitNormal) {
            damagedBone = null;
            localHitPos = default;
            hitNormal = default;

            ref PedHitData hitData = ref entity.GetComponent<PedHitData>(out bool hasHitData);
            if (!hasHitData) {
                sharedData.logger.WriteInfo($"No hit data at {entity.ID}");
                return false;
            }

            damagedBone = hitData.damagedBone;
            if (!damagedBone.IsValid) {
                sharedData.logger.WriteInfo("No valid bone");
                return false;
            }

            const float maxRange = 1f;
            Vector3 offset = damagedBone.GetPositionOffset(hitData.hitPos);
            if (offset.LengthSquared() > maxRange * maxRange) {
                sharedData.logger.WriteInfo("Exceed max range");
                return false;
            }

            localHitPos = offset;
            hitNormal = hitData.hitNorm;
            return true;
        }

        private void CleanFx() {
            foreach (EcsEntity entity in bleedToClean) {
                CleanEntity(entity);
            }
        }

        private ParticleEffect SpawnParticles(in Bleeding bleeding, PedBone targetBone, Vector3 localHitPos, Vector3 localRot) {
            ParticleEffectAsset asset;
            string effectName;
            if (bleeding.causedByPenetration) {
                GetEffectForPenetrationBleeding(bleeding.severity, out asset, out effectName);
            } else {
                GetEffectForBluntBleeding(bleeding.severity, out asset, out effectName);
            }

            if (string.IsNullOrEmpty(effectName)) {
                return null;
            }

#if DEBUG && DEBUG_EVERY_FRAME
            var message = $"Spawn particle {asset.AssetName}:{effectName} at {targetBone.Owner.Handle}({targetBone.Name})";
            sharedData.logger.WriteInfo(message);
            sharedData.logger.WriteInfo($"LocalPos:{localHitPos} LocalRot:{localRot}");
#endif

            return GTA.World.CreateParticleEffect(asset, effectName, targetBone, localHitPos, localRot);
        }

        private void GetEffectForPenetrationBleeding(float severity, out ParticleEffectAsset asset, out string effectName) {
            if (severity > 0.5f) {
                asset = cutMichael2Asset;
                effectName = "cs_mich2_blood_head_leak";
            } else if (severity > 0.2f) {
                asset = coreAsset;
                effectName = "ped_blood_drips";
            } else if (severity > 0.05f) {
                asset = cutSecAsset;
                effectName = "cut_sec_golfclub_blood_drips";
            } else {
                asset = default;
                effectName = null;
            }
        }

        private void GetEffectForBluntBleeding(float severity, out ParticleEffectAsset asset, out string effectName) {
            if (severity > 0.1f) {
                asset = coreAsset;
                effectName = "ped_blood_drips";
            } else if (severity > 0.01f) {
                asset = cutSecAsset;
                effectName = "cut_sec_golfclub_blood_drips";
            } else {
                asset = default;
                effectName = null;
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