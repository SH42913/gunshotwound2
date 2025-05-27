// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using Utils;

    public sealed class BodyPartConfig {
        public readonly struct BodyPart {
            public readonly string Key;
            public readonly string LocKey;
            public readonly HashSet<int> Bones;
            public readonly (string key, int weight)[] BluntTraumas;
            public readonly (string key, int weight)[] PenetratingTraumas;
            public readonly float TraumaChance;

            public bool IsValid => !string.IsNullOrEmpty(Key);

            public BodyPart(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                Bones = node.GetString(nameof(Bones))
                            .Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                            .Select(GetIntOfBone)
                            .ToHashSet();

                BluntTraumas = ExtractTraumas(node.Element(nameof(BluntTraumas)));
                PenetratingTraumas = ExtractTraumas(node.Element(nameof(PenetratingTraumas)));
                TraumaChance = node.GetFloat(nameof(TraumaChance));
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
        }

        public BodyPart[] BodyParts;

        public void FillFrom(XDocument doc) {
            XElement partsNode = doc.Element(nameof(BodyParts))!;
            BodyParts = partsNode.Elements(nameof(BodyPart)).Select(x => new BodyPart(x)).ToArray();
        }

        public BodyPart GetBodyPartByBone(Bone bone) {
            return GetBodyPartByBoneIndex((int)bone);
        }

        public BodyPart GetBodyPartByBoneIndex(int index) {
            foreach (BodyPart part in BodyParts) {
                if (part.Bones.Contains(index)) {
                    return part;
                }
            }

            throw new Exception($"There's no BodyPart for index {index}");
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