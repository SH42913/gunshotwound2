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
                if (hitData.bodyPart != PedHitData.BodyParts.Nothing) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
                if (hitData.randomBodyPart) {
                    int index = sharedData.random.Next(1, PARTS.Length);
                    hitData.bodyPart = PARTS[index];
#if DEBUG
                    sharedData.logger.WriteInfo($"Damaged random part is {hitData.bodyPart} of {convertedPed.name}");
#endif
                } else {
                    hitData.bodyPart = GetDamagedBodyPart(ref convertedPed);
                }
            }
        }

        private unsafe PedHitData.BodyParts GetDamagedBodyPart(ref ConvertedPed target) {
            var damagedBoneNum = 0;
            int* x = &damagedBoneNum;
            Function.Call(Hash.GET_PED_LAST_DAMAGE_BONE, target.thisPed, x);
            if (!Enum.TryParse(damagedBoneNum.ToString(), out Bone damagedBone)) {
                sharedData.logger.WriteError($"Can't parse bone {damagedBone}");
                return PedHitData.BodyParts.Nothing;
            }

            PedHitData.BodyParts damagePart = default;
            switch (damagedBone) {
                case Bone.SkelHead:
                    damagePart = PedHitData.BodyParts.Head;
                    break;
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

            if (damagePart == PedHitData.BodyParts.Nothing) {
                sharedData.logger.WriteError($"Can't detect part by bone {damagedBone}");
            } else {
#if DEBUG
                sharedData.logger.WriteInfo($"Damaged part is {damagePart}, bone {damagedBone} at {target.name}");
#endif
            }

            return damagePart;
        }
    }
}