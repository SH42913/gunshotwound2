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
                    var message = $"Damaged random part is {hitData.bodyPart.Key}, bone {randomBoneTag} of {convertedPed.name}";
                    sharedData.logger.WriteInfo(message);
#endif
                } else {
                    hitData.damagedBone = damagedBone;
                    hitData.bodyPart = BodyPartConfig.GetBodyPartByBone(damagedBone.Tag);
#if DEBUG
                    var message = $"Damaged part is {hitData.bodyPart.Key}, bone {damagedBone.Name} at {convertedPed.name}";
                    sharedData.logger.WriteInfo(message);
#endif
                }

                CalculateLocalHitData(convertedPed, ref hitData);
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

        private static void CalculateLocalHitData(in ConvertedPed convertedPed, ref PedHitData hitData) {
            if (hitData.weaponHash == (uint)WeaponHash.Unarmed) {
                return;
            }

            Ped aggressor = hitData.aggressor;
            if (!aggressor.IsValid()) {
                return;
            }

            Vector3 lastWorldHit = aggressor.LastWeaponImpactPosition;
            if (lastWorldHit == Vector3.Zero) {
                return;
            }

            DamageType damageType = GTAHelpers.GetWeaponDamageType(hitData.weaponHash);
            if (damageType == DamageType.Melee) {
                hitData.hitPos = lastWorldHit;
                hitData.hitNorm = (lastWorldHit - hitData.damagedBone.Position).Normalized;
                hitData.fullHitData = true;
            } else if (damageType == DamageType.Bullet) {
                Vector3 raycastOrigin = aggressor.IsPlayer
                        ? GameplayCamera.Position
                        : GTAHelpers.GetPedBoneCoords(aggressor, Bone.IKRightHand);

                hitData.shotDir = (lastWorldHit - raycastOrigin).Normalized;
                Vector3 raycastTarget = lastWorldHit + 50f * hitData.shotDir;
                RaycastResult result = GTA.World.Raycast(raycastOrigin, raycastTarget, IntersectFlags.Ragdolls, aggressor);
                if (result.DidHit && result.HitEntity == convertedPed.thisPed) {
                    hitData.hitPos = result.HitPosition + 0.0075f * hitData.shotDir;
                    hitData.hitNorm = result.SurfaceNormal.Normalized;
                    hitData.fullHitData = true;
                }

#if DEBUG && DEBUG_EVERY_FRAME
                RaycastDebugDrawer.RegisterRay(raycastOrigin, raycastTarget, [result.HitPosition]);
#endif
            }
        }
    }
}