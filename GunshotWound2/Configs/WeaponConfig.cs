// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using HitDetection;
    using SHVDN;
    using Utils;

    public sealed class WeaponConfig {
        public readonly struct Stats {
            public readonly string Key;
            public readonly HashSet<uint> Hashes;
            public readonly (string key, int weight)[] Wounds;
            public readonly float DamageMult;
            public readonly float BleedMult;
            public readonly float PainMult;
            public readonly float CritChance;
            public readonly int ArmorDamage;
            public readonly bool CanPenetrateArmor;
            public readonly float HelmetSafeChance;

            public bool IsValid => !string.IsNullOrEmpty(Key);

            public Stats(string key,
                         HashSet<uint> hashes,
                         (string key, int weight)[] wounds,
                         float damageMult,
                         float bleedMult,
                         float painMult,
                         float critChance,
                         int armorDamage,
                         bool canPenetrateArmor,
                         float helmetSafeChance) {
                Key = key;
                Hashes = hashes;
                Wounds = wounds;
                DamageMult = damageMult;
                BleedMult = bleedMult;
                PainMult = painMult;
                CritChance = critChance;
                ArmorDamage = armorDamage;
                CanPenetrateArmor = canPenetrateArmor;
                HelmetSafeChance = helmetSafeChance;
            }
        }

        private const string STUN_KEY = "Stun";

        public bool UseSpecialStunDamage;
        public HashSet<uint> IgnoreSet;
        public Stats[] AllWeapons;

        public Stats StunStats => AllWeapons.First(x => x.Key == STUN_KEY);
        public Stats LightImpactStats => AllWeapons.First(x => x.Key == "LightImpact");
        public Stats HeavyImpactStats => AllWeapons.First(x => x.Key == "HeavyImpact");
        public Stats CuttingStats => AllWeapons.First(x => x.Key == "Cutting");

        public void FillFrom(XDocument doc, ILogger logger) {
            XElement node = doc.Element(nameof(WeaponConfig))!;
            UseSpecialStunDamage = node.Element(nameof(UseSpecialStunDamage)).GetBool();
            IgnoreSet = ExtractWeaponHashes(node.Element(nameof(IgnoreSet)), logger);

            XElement statsNode = node.Element(nameof(Stats))!;
            AllWeapons = statsNode.Elements().Select(x => GetStatsForWeapon(x, logger)).ToArray();

            SuggestWeapons(logger);
        }

        private void SuggestWeapons(ILogger logger) {
            HashSet<uint>[] allHashSets = AllWeapons
                                          .Select(x => x.Hashes)
                                          .Append(IgnoreSet)
                                          .ToArray();

            foreach (uint hash in NativeMemory.GetAllWeaponHashesForHumanPeds()) {
                var alreadyRegistered = false;
                foreach (HashSet<uint> set in allHashSets) {
                    if (set.Contains(hash)) {
                        alreadyRegistered = true;
                        break;
                    }
                }

                if (!alreadyRegistered) {
                    logger.WriteInfo($"Weapon {BuildWeaponName(hash)} can be added to GSW2");
                }
            }
        }

        private static Stats GetStatsForWeapon(XElement weaponNode, ILogger logger) {
            string name = weaponNode.Name.LocalName;
            HashSet<uint> hashes = ExtractWeaponHashes(weaponNode, logger);
            (string, int)[] wounds = ExtractWounds(weaponNode);

            return new Stats(name,
                             hashes,
                             wounds,
                             weaponNode.GetFloat(nameof(Stats.DamageMult), defaultValue: 1f),
                             weaponNode.GetFloat(nameof(Stats.BleedMult), defaultValue: 1f),
                             weaponNode.GetFloat(nameof(Stats.PainMult), defaultValue: 1f),
                             weaponNode.GetFloat(nameof(Stats.CritChance), defaultValue: 0f),
                             weaponNode.GetInt(nameof(Stats.ArmorDamage), defaultValue: 0),
                             weaponNode.GetBool(nameof(Stats.CanPenetrateArmor), defaultValue: false),
                             weaponNode.GetFloat(nameof(Stats.HelmetSafeChance), defaultValue: 1f));
        }

        private static (string, int)[] ExtractWounds(XElement weaponNode) {
            XElement woundsNode = weaponNode.Element(nameof(Stats.Wounds))!;
            return woundsNode.Elements("Item")
                             .Select(x => (x.GetString("Key"), x.GetInt("Weight")))
                             .ToArray();
        }

        private static HashSet<uint> ExtractWeaponHashes(XElement node, ILogger logger) {
            var set = new HashSet<uint>();
            foreach (XElement element in node.Elements()) {
                const string attributeName = "Hashes";
                string attributeValue = element.GetString(attributeName);
                if (!string.IsNullOrEmpty(attributeValue)) {
                    ParseStringToSet(attributeValue);
                }
            }

            return set;

            void ParseStringToSet(string stringOfHashes) {
                string[] splitStrings = stringOfHashes.Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string weaponString in splitStrings) {
                    if (!uint.TryParse(weaponString, out uint hash)) {
                        const string prefix = "WEAPON_";
                        hash = StringHash.AtStringHashUtf8(prefix + weaponString.ToUpper());
                    }

                    if (NativeMemory.IsHashValidAsWeaponHash(hash)) {
                        set.Add(hash);
                    } else {
                        logger.WriteWarning($"{weaponString}({hash.ToString()}) is not valid weapon. GSW2 will ignore it.");
                    }
                }
            }
        }

        public static string BuildWeaponName(uint hash) {
            string name = Weapon.GetHumanNameFromHash((WeaponHash)hash);
            return $"{name}({hash.ToString()})";
        }
    }
}