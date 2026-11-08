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
        private AspectFactory<PedWithHit> damagedPedAspect;

        public EcsWorld World { get; set; }

        private BodyPartConfig BodyPartConfig => sharedData.mainConfig.bodyPartConfig;

        public BodyHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.Extend<PedWithHit>();
            damagedPedAspect = World.GetAspectFactory<PedWithHit>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity pedEntity in damagedPeds) {
                PedWithHit aspect = damagedPedAspect.Get(pedEntity);
                ref PedHitData hitData = ref aspect.Hit;
                if (ShouldSkipDetection(ref hitData)) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref aspect.Ped;
                PedBone damagedBone = convertedPed.thisPed.Bones.LastDamaged;
                bool useRandomBone = hitData.weaponType.UseRandomBone || !damagedBone.IsValid;
                if (useRandomBone) {
                    hitData.bodyPart = sharedData.random.Next(BodyPartConfig.BodyParts);

                    var randomBoneTag = (Bone)sharedData.random.NextFromCollection(hitData.bodyPart.Bones);
                    hitData.damagedBone = convertedPed.thisPed.Bones[randomBoneTag];
                } else {
                    hitData.damagedBone = damagedBone;
                    hitData.bodyPart = BodyPartConfig.GetBodyPartByBone(damagedBone.Tag);
                }

#if DEBUG
                string random = useRandomBone ? "(random)" : "";
                var message =
                        $"Damaged body part is {hitData.bodyPart.Key}{random}, bone {hitData.damagedBone.Name} at {convertedPed.name}";

                sharedData.logger.WriteInfo(message);
#endif

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