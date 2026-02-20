// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using Utils;

    public sealed class BodyPartConfig : MainConfig.IConfig {
        public readonly struct BodyPart {
            private static readonly int[] HEAD_BONES = [(int)Bone.SkelHead,];

            private static readonly int[] LEFT_ARM_BONES = [
                (int)Bone.SkelLeftClavicle,
                (int)Bone.SkelLeftUpperArm,
                (int)Bone.SkelLeftForearm,
                (int)Bone.SkelLeftHand,
            ];

            private static readonly int[] RIGHT_ARM_BONES = [
                (int)Bone.SkelRightClavicle,
                (int)Bone.SkelRightUpperArm,
                (int)Bone.SkelRightForearm,
                (int)Bone.SkelRightHand,
            ];

            private static readonly int[] LEFT_LEG_BONES = [
                (int)Bone.SkelLeftThigh,
                (int)Bone.SkelLeftToe0,
                (int)Bone.SkelLeftToe1,
                (int)Bone.SkelLeftFoot,
                (int)Bone.SkelLeftCalf,
            ];

            private static readonly int[] RIGHT_LEG_BONES = [
                (int)Bone.SkelRightThigh,
                (int)Bone.SkelRightToe0,
                (int)Bone.SkelRightToe1,
                (int)Bone.SkelRightFoot,
                (int)Bone.SkelRightCalf,
            ];

            public readonly string Key;
            public readonly string LocKey;
            public readonly DBPContainer DBPMults;
            public readonly HashSet<int> Bones;
            public readonly float TangentialWoundThreshold;
            public readonly (string key, int weight)[] BluntTraumas;
            public readonly (string key, int weight)[] PenetratingTraumas;
            public readonly float TraumaChance;
            public readonly bool IgnoreWeaponTraumaChanceForPenetratingWounds;

            public bool IsValid => !string.IsNullOrEmpty(Key);

            public BodyPart(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                DBPMults = new DBPContainer(node, isMult: true);

                Bones = node.GetString(nameof(Bones))
                            .Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                            .Select(GetIntOfBone)
                            .ToHashSet();

                TangentialWoundThreshold = node.GetFloat(nameof(TangentialWoundThreshold), defaultValue: 0.3f);
                BluntTraumas = ExtractTraumas(node.Element(nameof(BluntTraumas)));
                PenetratingTraumas = ExtractTraumas(node.Element(nameof(PenetratingTraumas)));
                TraumaChance = node.GetFloat(nameof(TraumaChance));
                IgnoreWeaponTraumaChanceForPenetratingWounds = node.GetBool(nameof(IgnoreWeaponTraumaChanceForPenetratingWounds));
            }

            private static int GetIntOfBone(string boneName) {
                if (Enum.TryParse(boneName, ignoreCase: true, out Bone bone)) {
                    return (int)bone;
                } else {
                    throw new Exception($"Can't parse bone {boneName}");
                }
            }

            private static (string, int)[] ExtractTraumas(XElement node) {
                return node.Elements(nameof(TraumaConfig.Trauma))
                           .Select(x => (x.GetString(nameof(TraumaConfig.Trauma.Key)), x.GetInt(MainConfig.WEIGHT_ATTRIBUTE_NAME)))
                           .ToArray();
            }

            public bool IsHead() {
                return Bones.Overlaps(HEAD_BONES);
            }

            public bool IsLeftArm() {
                return Bones.Overlaps(LEFT_ARM_BONES);
            }

            public bool IsRightArm() {
                return Bones.Overlaps(RIGHT_ARM_BONES);
            }

            public bool IsLeftLeg() {
                return Bones.Overlaps(LEFT_LEG_BONES);
            }

            public bool IsRightLeg() {
                return Bones.Overlaps(RIGHT_LEG_BONES);
            }
        }

        public BodyPart[] BodyParts;

        public string sectionName => "BodyParts.xml";

        public void FillFrom(XDocument doc) {
            BodyParts = doc.Element(nameof(BodyParts))!
                           .Elements(nameof(BodyPart))
                           .Select(x => new BodyPart(x))
                           .ToArray();
        }

        public void Validate(MainConfig mainConfig, ILogger logger) {
            foreach (BodyPart bodyPart in BodyParts) {
                foreach ((string key, int _) in bodyPart.BluntTraumas) {
                    if (!IsValidTrauma(key)) {
                        logger.WriteWarning($"{bodyPart.Key} has invalid blunt trauma {key}");
                    }
                }

                foreach ((string key, int _) in bodyPart.PenetratingTraumas) {
                    if (!IsValidTrauma(key)) {
                        logger.WriteWarning($"{bodyPart.Key} has invalid penetrating trauma {key}");
                    }
                }
            }

            bool IsValidTrauma(string key) {
                return mainConfig.traumaConfig.Traumas.ContainsKey(key);
            }
        }

        public BodyPart GetBodyPartByBone(Bone bone) {
            foreach (BodyPart part in BodyParts) {
                if (part.Bones.Contains((int)bone)) {
                    return part;
                }
            }

            throw new Exception($"There's no BodyPart for bone {bone}");
        }

        public BodyPart GetBodyPartByKey(string key) {
            foreach (BodyPart bodyPart in BodyParts) {
                if (bodyPart.Key == key) {
                    return bodyPart;
                }
            }

            throw new Exception($"There's no BodyPart for key {key}");
        }
    }
}