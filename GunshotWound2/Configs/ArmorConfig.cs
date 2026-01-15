// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using Utils;

    public sealed class ArmorConfig : MainConfig.IConfig {
        private const int MIN_ARMOR_VALUE = 1;

        public readonly struct Level {
            public readonly string Key;
            public readonly string LocKey;
            public readonly int MaxValue;
            public readonly string ColorPrefix;
            public readonly HashSet<string> Parts;
            public readonly float TraumaPadEfficiency;

            public Level(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                MaxValue = node.GetInt(nameof(MaxValue));
                ColorPrefix = node.GetString(nameof(ColorPrefix));
                Parts = node.GetString(nameof(Parts))
                            .Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                            .ToHashSet();

                TraumaPadEfficiency = node.GetFloat(nameof(TraumaPadEfficiency));
            }
        }

        public float MinimalChanceForArmorSave;
        public HashSet<int> HelmetPropIndexes;
        public Level[] Levels;

        public string sectionName => "Armor.xml";

        public void FillFrom(XDocument doc) {
            XElement node = doc.Element(nameof(ArmorConfig))!;
            MinimalChanceForArmorSave = node.Element(nameof(MinimalChanceForArmorSave)).GetFloat();
            HelmetPropIndexes = ExtractHelmets(node);

            Levels = node.Element(nameof(Levels))!
                         .Elements(nameof(Level))
                         .Select(x => new Level(x))
                         .OrderBy(x => x.MaxValue)
                         .ToArray();
        }

        public void Validate(MainConfig mainConfig, ILogger logger) {
            if (Levels.Length < 1) {
                logger.WriteError("There's no any armor levels, GSW will not use armor feature");
                return;
            }

            foreach (Level level in Levels) {
                if (level.MaxValue < MIN_ARMOR_VALUE) {
                    logger.WriteWarning($"MaxValue for {level.Key} must be positive");
                }

                foreach (string key in level.Parts) {
                    bool isValidBodyPart = Array.Exists(mainConfig.bodyPartConfig.BodyParts, x => x.Key == key);
                    if (!isValidBodyPart) {
                        logger.WriteWarning($"{level.Key} has invalid body part {key}");
                    }
                }
            }
        }

        public bool PedHasHelmet(Ped ped) {
            int currentIndex = ped.Style[PedPropAnchorPoint.Head].Index;
            return HelmetPropIndexes.Contains(currentIndex);
        }

        public Level GetArmorLevelByKey(string key) {
            foreach (Level level in Levels) {
                if (level.Key == key) {
                    return level;
                }
            }

            throw new Exception($"There's no level with key {key}");
        }

        public bool TryGetArmorLevel(int armor, out Level level) {
            if (armor < MIN_ARMOR_VALUE) {
                level = default;
                return false;
            }

            foreach (Level armorLevel in Levels) {
                if (armor <= armorLevel.MaxValue) {
                    level = armorLevel;
                    return true;
                }
            }

            level = Levels[Levels.Length - 1];
            return true;
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