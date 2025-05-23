// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using Utils;

    public sealed class ArmorConfig {
        public readonly struct Level {
            public readonly string Key;
            public readonly string LocKey;
            public readonly int MaxValue;
            public readonly string ColorPrefix;

            public Level(string key, string locKey, int maxValue, string colorPrefix) {
                Key = key;
                LocKey = locKey;
                MaxValue = maxValue;
                ColorPrefix = colorPrefix;
            }

            public Level(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                MaxValue = node.GetInt(nameof(MaxValue));
                ColorPrefix = node.GetString(nameof(ColorPrefix));
            }
        }

        public float MinimalChanceForArmorSave;
        public HashSet<int> HelmetPropIndexes;
        public Level[] Levels;

        public void FillFrom(XDocument doc) {
            XElement node = doc.Element(nameof(ArmorConfig))!;
            MinimalChanceForArmorSave = node.Element(nameof(MinimalChanceForArmorSave)).GetFloat();
            HelmetPropIndexes = ExtractHelmets(node);

            XElement levelsNode = node.Element(nameof(Levels))!;
            Levels = levelsNode.Elements(nameof(Level)).Select(x => new Level(x)).OrderBy(x => x.MaxValue).ToArray();
        }

        public bool PedHasHelmet(Ped ped) {
            int currentIndex = ped.Style[PedPropAnchorPoint.Head].Index;
            return HelmetPropIndexes.Contains(currentIndex);
        }

        public bool TryGetArmorLevel(Ped ped, out Level level) {
            int armor = ped.Armor;
            if (armor < 1) {
                level = default;
                return false;
            }

            foreach (Level armorLevel in Levels) {
                if (armor <= armorLevel.MaxValue) {
                    level = armorLevel;
                    return true;
                }
            }

            throw new Exception($"There's no level for armor {armor}");
        }

        private static HashSet<int> ExtractHelmets(XElement node) {
            string indexesString = node?.Element(nameof(HelmetPropIndexes))?.Attribute("Indexes")?.Value;
            return string.IsNullOrEmpty(indexesString)
                    ? new HashSet<int>()
                    : indexesString
                      .Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToHashSet();
        }
    }
}