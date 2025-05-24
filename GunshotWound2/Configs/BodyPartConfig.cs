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

            public bool IsValid => !string.IsNullOrEmpty(Key);

            public BodyPart(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                Bones = node.GetString(nameof(Bones))
                            .Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                            .Select(GetIntOfBone)
                            .ToHashSet();
            }

            private static int GetIntOfBone(string boneName) {
                if (Enum.TryParse(boneName, ignoreCase: true, out Bone bone)) {
                    return (int)bone;
                } else {
                    throw new Exception($"Can't parse bone {boneName}");
                }
            }
        }

        public BodyPart[] BodyParts;

        public void FillFrom(XDocument doc) {
            XElement partsNode = doc.Element(nameof(BodyParts))!;
            BodyParts = partsNode.Elements(nameof(BodyPart)).Select(x => new BodyPart(x)).ToArray();
        }

        public bool TryGetBodyPartByBone(Bone bone, out BodyPart bodyPart) {
            return TryGetBodyPartByBoneIndex((int)bone, out bodyPart);
        }

        public bool TryGetBodyPartByBoneIndex(int index, out BodyPart bodyPart) {
            foreach (BodyPart part in BodyParts) {
                if (part.Bones.Contains(index)) {
                    bodyPart = part;
                    return true;
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