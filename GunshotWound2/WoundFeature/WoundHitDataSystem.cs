namespace GunshotWound2.WoundFeature {
    using System;
    using GTA;
    using GTA.Math;
    using HitDetection;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class WoundHitDataSystem : ILateSystem {
        // ReSharper disable once NotAccessedField.Local
        private readonly SharedData sharedData;

        private Filter woundsToInit;
        private Stash<WoundData> woundDataStash;
        private Stash<PedHitData> hitStash;

        public EcsWorld World { get; set; }

        public WoundHitDataSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            woundsToInit = World.Filter.With<WoundData>().With<PedHitData>();

            woundDataStash = World.GetStash<WoundData>();
            hitStash = World.GetStash<PedHitData>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in woundsToInit) {
                ref WoundData woundData = ref woundDataStash.Get(entity);
                ref PedHitData hitData = ref hitStash.Get(entity);
                woundData.damagedBone = hitData.damagedBone;
                if (woundData.damagedBone == null || !woundData.damagedBone.IsValid) {
                    int randomBoneId = sharedData.random.NextFromCollection(hitData.bodyPart.Bones);
                    int boneIndex = PedEffects.GetBoneIndex(hitData.ped, randomBoneId);
                    woundData.damagedBone = hitData.ped.Bones[boneIndex];
#if DEBUG
                    sharedData.logger.WriteInfo($"There's no bone in hit, so will be used random - {(Bone)randomBoneId}");
#endif
                }

                if (hitData.fullHitData) {
                    Vector3 localHitPos = woundData.damagedBone.GetPositionOffset(hitData.hitPos);

                    const float maxRange = 1f;
                    if (localHitPos.LengthSquared() > maxRange * maxRange) {
#if DEBUG
                        sharedData.logger.WriteInfo("Local hit pos exceeds max range");
#endif
                        continue;
                    }

                    Quaternion invertedBoneQuat = Quaternion.Invert(woundData.damagedBone.Quaternion);
                    woundData.localHitNormal = (invertedBoneQuat * hitData.hitNorm).Normalized;
                    woundData.localHitPos = localHitPos;
                    woundData.hasHitData = true;
                }

                hitStash.Remove(entity);
            }
        }

        void IDisposable.Dispose() { }
    }
}