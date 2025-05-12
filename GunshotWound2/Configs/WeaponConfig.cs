// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    public sealed class WeaponConfig {
        public HashSet<uint> SmallCaliberHashes;
        public HashSet<uint> MediumCaliberHashes;
        public HashSet<uint> HeavyCaliberHashes;
        public HashSet<uint> LightImpactHashes;
        public HashSet<uint> HeavyImpactHashes;
        public HashSet<uint> ShotgunHashes;
        public HashSet<uint> CuttingHashes;
        public HashSet<uint> IgnoreHashes;

        public Dictionary<string, float?[]> DamageSystemConfigs;

        public void FillFrom(XElement doc) {
            XElement node = doc.Element("Weapons");
            if (node == null) {
                return;
            }

            SmallCaliberHashes = ExtractWeaponHashes(node, "SmallCaliber");
            MediumCaliberHashes = ExtractWeaponHashes(node, "MediumCaliber");
            HeavyCaliberHashes = ExtractWeaponHashes(node, "HighCaliber");
            LightImpactHashes = ExtractWeaponHashes(node, "LightImpact");
            HeavyImpactHashes = ExtractWeaponHashes(node, "HeavyImpact");
            ShotgunHashes = ExtractWeaponHashes(node, "Shotgun");
            CuttingHashes = ExtractWeaponHashes(node, "Cutting");
            IgnoreHashes = ExtractWeaponHashes(node, "Ignore");

            DamageSystemConfigs = ExtractDamageSystemConfigs(node);
        }

        private static HashSet<uint> ExtractWeaponHashes(XElement node, string weaponName) {
            XElement weaponNode = node.Element(weaponName);
            if (weaponNode == null) {
                throw new Exception($"{weaponName} node not found!");
            }

            const string name = "Hashes";
            string hashes = weaponNode.Element(name)?.Attribute(name)?.Value;
            if (string.IsNullOrEmpty(hashes)) {
                throw new Exception($"{weaponName}'s hashes is empty");
            }

            return hashes.Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries).Select(uint.Parse).ToHashSet();
        }

        private static Dictionary<string, float?[]> ExtractDamageSystemConfigs(XElement node) {
            var dictionary = new Dictionary<string, float?[]>();
            foreach (XElement element in node.Elements()) {
                var multipliers = new float?[5];

                XAttribute damageString = element.Attribute("DamageMult");
                multipliers[0] = damageString != null
                        ? float.Parse(damageString.Value, CultureInfo.InvariantCulture)
                        : null;

                XAttribute bleedingString = element.Attribute("BleedingMult");
                multipliers[1] = bleedingString != null
                        ? float.Parse(bleedingString.Value, CultureInfo.InvariantCulture)
                        : null;

                XAttribute painString = element.Attribute("PainMult");
                multipliers[2] = painString != null
                        ? float.Parse(painString.Value, CultureInfo.InvariantCulture)
                        : null;

                XAttribute critString = element.Attribute("CritChance");
                multipliers[3] = critString != null
                        ? float.Parse(critString.Value, CultureInfo.InvariantCulture)
                        : null;

                XAttribute armorString = element.Attribute("ArmorDamage");
                multipliers[4] = armorString != null
                        ? float.Parse(armorString.Value, CultureInfo.InvariantCulture)
                        : null;

                dictionary.Add(element.Name.LocalName, multipliers);
            }

            return dictionary;
        }
    }
}