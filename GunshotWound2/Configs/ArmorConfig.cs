// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using Utils;

    public sealed class ArmorConfig {
        public float MinimalChanceForArmorSave;
        public HashSet<int> HelmetPropIndexes;

        public void FillFrom(XElement doc) {
            XElement node = doc.Element("Armor");
            if (node == null) {
                return;
            }

            MinimalChanceForArmorSave = node.Element("MinimalChanceForArmorSave").GetFloat();
            HelmetPropIndexes = ExtractHelmets(node);
        }

        public bool PedHasHelmet(Ped ped) {
            int currentIndex = ped.Style[PedPropAnchorPoint.Head].Index;
            return HelmetPropIndexes.Contains(currentIndex);
        }

        private static HashSet<int> ExtractHelmets(XElement node) {
            string indexesString = node?.Element("HelmetPropIndexes")?.Attribute("Indexes")?.Value;
            return string.IsNullOrEmpty(indexesString)
                    ? new HashSet<int>()
                    : indexesString
                      .Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                      .Select(int.Parse)
                      .ToHashSet();
        }
    }
}