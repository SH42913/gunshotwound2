namespace GunshotWound2.HitDetection {
    using System;
    using Configs;
    using GTA;
    using GTA.Math;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class BodyHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        private BodyPartConfig BodyPartConfig => sharedData.mainConfig.bodyPartConfig;

        public BodyHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity pedEntity in damagedPeds) {
                ref PedHitData hitData = ref pedEntity.GetComponent<PedHitData>();
                if (ShouldSkipDetection(ref hitData)) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
                PedBone damagedBone = convertedPed.thisPed.Bones.LastDamaged;
                if (hitData.useRandomBodyPart || !damagedBone.IsValid) {
                    hitData.bodyPart = sharedData.random.Next(BodyPartConfig.BodyParts);

                    Bone randomBoneTag = (Bone)sharedData.random.NextFromCollection(hitData.bodyPart.Bones);
                    hitData.damagedBone = convertedPed.thisPed.Bones[randomBoneTag];
#if DEBUG
                    sharedData.logger.WriteInfo($"Damaged random part is {hitData.bodyPart.Key}, bone {randomBoneTag} of {convertedPed.name}");
#endif
                } else {
                    hitData.damagedBone = damagedBone;
                    hitData.bodyPart = BodyPartConfig.GetBodyPartByBone(damagedBone.Tag);
#if DEBUG
                    sharedData.logger.WriteInfo($"Damaged part is {hitData.bodyPart.Key}, bone {damagedBone.Tag} at {convertedPed.name}");
#endif
                }

                CalculateLocalHitPos(convertedPed, ref hitData);
                convertedPed.lastDamagedBone = hitData.damagedBone.Tag;
            }
        }

        private bool ShouldSkipDetection(ref PedHitData hitData) {
            if (!hitData.weaponType.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip body part detection, 'cause there's no weapon");
#endif
                return true;
            }

            if (hitData.bodyPart.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip body part detection, 'cause it's already detected");
#endif
                return true;
            }

            return false;
        }

        private void CalculateLocalHitPos(in ConvertedPed convertedPed, ref PedHitData hitData) {
            if (!hitData.aggressor.IsValid()) {
                return;
            }

            Vector3 lastHit = hitData.aggressor.LastWeaponImpactPosition;
            if (lastHit == Vector3.Zero) {
                return;
            }

            hitData.hitPos = lastHit;

            bool assignedNormal = false;
            Prop currentWeaponObject = hitData.aggressor.Weapons.CurrentWeaponObject;
            EntityBone muzzleBone = currentWeaponObject?.Bones["Gun_Muzzle"];
            if (muzzleBone != null && muzzleBone.IsValid) {
                hitData.shotDir = (lastHit - muzzleBone.Position).Normalized;

                RaycastResult result = GTA.World.Raycast(muzzleBone.Position, lastHit + hitData.shotDir, IntersectFlags.Peds);
                if (result.Result == 2 && result.HitEntity == convertedPed.thisPed) {
                    hitData.hitNorm = result.SurfaceNormal;
                    assignedNormal = true;
                } else {
                    sharedData.logger.WriteInfo("No hit");
                }
            } else {
                sharedData.logger.WriteInfo("No muzzle");
            }

            if (!assignedNormal) {
                hitData.hitNorm = (lastHit - hitData.damagedBone.Position).Normalized;
            }
        }
    }
}