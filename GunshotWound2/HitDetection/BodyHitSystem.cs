namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using GTA.Native;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class BodyHitSystem : ISystem {
        private static readonly PedHitData.BodyParts[] PARTS = (PedHitData.BodyParts[])Enum.GetValues(typeof(PedHitData.BodyParts));

        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

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
                if (hitData.useRandomBodyPart) {
                    int index = sharedData.random.Next(1, PARTS.Length);
                    hitData.bodyPart = PARTS[index];
#if DEBUG
                    sharedData.logger.WriteInfo($"Damaged random part is {hitData.bodyPart} of {convertedPed.name}");
#endif
                } else {
                    hitData.bodyPart = GetDamagedBodyPart(convertedPed.thisPed, out Bone damagedBone);
                    hitData.damagedBone = damagedBone;
                    if (hitData.bodyPart == PedHitData.BodyParts.Nothing) {
                        sharedData.logger.WriteError($"Can't detect part by bone {damagedBone}");
                    } else {
#if DEBUG
                        sharedData.logger.WriteInfo($"Damaged part is {hitData.bodyPart}, bone {damagedBone} at {convertedPed.name}");
#endif
                    }
                }
            }
        }

        private bool ShouldSkipDetection(ref PedHitData hitData) {
            if (hitData.weaponType == PedHitData.WeaponTypes.Nothing) {
#if DEBUG
                sharedData.logger.WriteWarning("Skip body part detection, 'cause there's no weapon");
#endif
                return true;
            }

            if (hitData.weaponType == PedHitData.WeaponTypes.Stun) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip body part detection, 'cause stun weapon don't need that");
#endif
                return true;
            }

            if (hitData.bodyPart != PedHitData.BodyParts.Nothing) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip body part detection, 'cause it's already detected");
#endif
                return true;
            }

            return false;
        }

        // ReSharper disable once CyclomaticComplexity
        private unsafe PedHitData.BodyParts GetDamagedBodyPart(Ped ped, out Bone damagedBone) {
            var damagedBoneNum = 0;
            int* x = &damagedBoneNum;
            Function.Call(Hash.GET_PED_LAST_DAMAGE_BONE, ped, x);
            if (!Enum.TryParse(damagedBoneNum.ToString(), out damagedBone)) {
                sharedData.logger.WriteError($"Can't parse bone {damagedBone}");
                return PedHitData.BodyParts.Nothing;
            }

            PedHitData.BodyParts damagePart = default;
            switch (damagedBone) {
                case Bone.SkelHead: damagePart = PedHitData.BodyParts.Head; break;
                case Bone.SkelNeck1:
                case Bone.SkelNeck2:
                    damagePart = PedHitData.BodyParts.Neck;
                    break;
                case Bone.SkelSpine2:
                case Bone.SkelSpine3:
                    damagePart = PedHitData.BodyParts.UpperBody;
                    break;
                case Bone.SkelRoot:
                case Bone.SkelSpineRoot:
                case Bone.SkelSpine0:
                case Bone.SkelSpine1:
                case Bone.SkelPelvis:
                case Bone.SkelPelvis1:
                case Bone.SkelPelvisRoot:
                    damagePart = PedHitData.BodyParts.LowerBody;
                    break;
                case Bone.SkelLeftThigh:
                case Bone.SkelRightThigh:
                case Bone.SkelLeftToe0:
                case Bone.SkelLeftToe1:
                case Bone.SkelRightToe0:
                case Bone.SkelRightToe1:
                case Bone.SkelLeftFoot:
                case Bone.SkelRightFoot:
                case Bone.SkelLeftCalf:
                case Bone.SkelRightCalf:
                    damagePart = PedHitData.BodyParts.Leg;
                    break;
                case Bone.SkelLeftUpperArm:
                case Bone.SkelRightUpperArm:
                case Bone.SkelLeftClavicle:
                case Bone.SkelRightClavicle:
                case Bone.SkelLeftForearm:
                case Bone.SkelRightForearm:
                case Bone.SkelLeftHand:
                case Bone.SkelRightHand:
                    damagePart = PedHitData.BodyParts.Arm;
                    break;
            }

            return damagePart;
        }
    }
}