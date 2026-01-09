namespace GunshotWound2.Utils {
    using GTA;
    using GTA.Math;

    public static class NMHelper {
        public static (Vector3 localNmPos, Vector3 localNmNormal, int nmIndex) GetNaturalMotionData(PedBone bone,
            Vector3 localOffset,
            Vector3 localNormal) {
            int nmIndex = GetNaturalMotionIndex(bone.Tag);
            if (nmIndex == -1) {
                return default;
            }

            Bone baseBone = GetBoneForNaturalMotionIndex(nmIndex);
            if (baseBone == Bone.Invalid) {
                return default;
            }

            Vector3 worldPoint = bone.GetOffsetPosition(localOffset);
            Vector3 worldNormal = bone.PoseMatrix.TransformDirection(localNormal);

            PedBone targetBone = bone.Owner.Bones[baseBone];
            Vector3 localNmPos = targetBone.GetPositionOffset(worldPoint);
            Vector3 localNmNormal = targetBone.PoseMatrix.InverseTransformDirection(worldNormal);
            localNmNormal.Normalize();
            return (localNmPos, localNmNormal, nmIndex);
        }

        // ReSharper disable once CyclomaticComplexity
        private static int GetNaturalMotionIndex(Bone bone) {
            switch (bone) {
                case Bone.SkelRoot:
                case Bone.SkelSpineRoot:
                case Bone.SkelPelvisRoot:
                case Bone.SkelPelvis:
                case Bone.SkelPelvis1:
                    return 0;

                case Bone.SkelSpine0: return 7;
                case Bone.SkelSpine1: return 8;
                case Bone.SkelSpine2: return 9;
                case Bone.SkelSpine3: return 10;

                case Bone.SkelLeftClavicle: return 11;
                case Bone.SkelLeftUpperArm: return 12;
                case Bone.SkelLeftForearm:  return 13;
                case Bone.SkelLeftHand:     return 14;

                case Bone.SkelRightClavicle: return 15;
                case Bone.SkelRightUpperArm: return 16;
                case Bone.SkelRightForearm:  return 17;
                case Bone.SkelRightHand:     return 18;

                case Bone.SkelLeftThigh: return 1;
                case Bone.SkelLeftCalf:  return 2;
                case Bone.SkelLeftFoot:
                case Bone.SkelLeftToe0:
                case Bone.SkelLeftToe1:
                    return 3;

                case Bone.SkelRightThigh: return 4;
                case Bone.SkelRightCalf:  return 5;
                case Bone.SkelRightFoot:
                case Bone.SkelRightToe0:
                case Bone.SkelRightToe1:
                    return 6;

                case Bone.SkelNeck1:
                case Bone.SkelNeck2:
                    return 19;
                case Bone.SkelHead: return 20;

                default: return -1;
            }
        }

        // ReSharper disable once CyclomaticComplexity
        private static Bone GetBoneForNaturalMotionIndex(int nmIndex) {
            return nmIndex switch {
                0  => Bone.SkelPelvis,
                1  => Bone.SkelLeftThigh,
                2  => Bone.SkelLeftCalf,
                3  => Bone.SkelLeftFoot,
                4  => Bone.SkelRightThigh,
                5  => Bone.SkelRightCalf,
                6  => Bone.SkelRightFoot,
                7  => Bone.SkelSpine0,
                8  => Bone.SkelSpine1,
                9  => Bone.SkelSpine2,
                10 => Bone.SkelSpine3,
                11 => Bone.SkelLeftClavicle,
                12 => Bone.SkelLeftUpperArm,
                13 => Bone.SkelLeftForearm,
                14 => Bone.SkelLeftHand,
                15 => Bone.SkelRightClavicle,
                16 => Bone.SkelRightUpperArm,
                17 => Bone.SkelRightForearm,
                18 => Bone.SkelRightHand,
                19 => Bone.SkelNeck1,
                20 => Bone.SkelHead,
                _  => Bone.Invalid,
            };
        }
    }
}