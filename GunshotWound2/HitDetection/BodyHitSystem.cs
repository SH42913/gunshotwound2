// #define DEBUG_EVERY_FRAME

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
                    sharedData.logger.WriteInfo($"Damaged part is {hitData.bodyPart.Key}, bone {damagedBone.Name} at {convertedPed.name}");
#endif
                }

                CalculateLocalHitData(convertedPed, ref hitData);
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

        private void CalculateLocalHitData(in ConvertedPed convertedPed, ref PedHitData hitData) {
            Ped aggressor = hitData.aggressor;
            if (!aggressor.IsValid()) {
                return;
            }

            DamageType damageType = GTAHelpers.GetWeaponDamageType(hitData.weaponHash);
            if (damageType != DamageType.Bullet && damageType != DamageType.Melee) {
                return;
            }

            Vector3 lastHit = aggressor.LastWeaponImpactPosition;
            if (lastHit == Vector3.Zero) {
                return;
            }

            hitData.hitPos = lastHit;

            Vector3 raycastOrigin = GTAHelpers.GetPedBoneCoords(aggressor, Bone.IKRightHand);
            if (raycastOrigin == Vector3.Zero) {
#if DEBUG && DEBUG_EVERY_FRAME
                sharedData.logger.WriteInfo("Invalid raycast origin");
#endif
                return;
            }

            Ped thisPed = convertedPed.thisPed;
            hitData.shotDir = (lastHit - raycastOrigin).Normalized;
            Vector3 raycastTarget = lastHit + 5f * hitData.shotDir;
            RaycastResult result = GTA.World.Raycast(raycastOrigin, raycastTarget, IntersectFlags.Peds, aggressor);
            if (result.Result == 2 && result.HitEntity == thisPed) {
                hitData.hitNorm = result.SurfaceNormal;
            } else {
#if DEBUG && DEBUG_EVERY_FRAME
                sharedData.logger.WriteInfo("No raycast hit for local hit calculation");
#endif
                hitData.hitNorm = (lastHit - hitData.damagedBone.Position).Normalized;
            }
        }
    }
}