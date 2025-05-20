// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using GTA;
    using HitDetection;
    using SHVDN;
    using Utils;

    public sealed class WeaponConfig {
        public readonly struct Stats {
            public readonly PedHitData.WeaponTypes Type;
            public readonly HashSet<uint> Hashes;
            public readonly float DamageMult;
            public readonly float BleedMult;
            public readonly float PainMult;
            public readonly float CritChance;
            public readonly int ArmorDamage;
            public readonly bool CanPenetrateArmor;
            public readonly float HelmetSafeChance;

            public Stats(PedHitData.WeaponTypes type,
                         HashSet<uint> hashes,
                         float damageMult,
                         float bleedMult,
                         float painMult,
                         float critChance,
                         int armorDamage,
                         bool canPenetrateArmor,
                         float helmetSafeChance) {
                Type = type;
                Hashes = hashes;
                DamageMult = damageMult;
                BleedMult = bleedMult;
                PainMult = painMult;
                CritChance = critChance;
                ArmorDamage = armorDamage;
                CanPenetrateArmor = canPenetrateArmor;
                HelmetSafeChance = helmetSafeChance;
            }
        }

        public bool UseSpecialStunDamage;
        public HashSet<uint> IgnoreHashes;

        public Stats SmallCaliber;
        public Stats MediumCaliber;
        public Stats HeavyCaliber;
        public Stats LightImpact;
        public Stats HeavyImpact;
        public Stats Shotgun;
        public Stats Cutting;

        public Stats[] AllStats;

        public void FillFrom(XDocument doc, ILogger logger) {
            XElement node = doc.Element("Weapons");
            if (node == null) {
                return;
            }

            UseSpecialStunDamage = node.Element("UseSpecialStunDamage").GetBool();
            IgnoreHashes = ExtractWeaponHashes(node.Element("Ignore"), logger);

            XElement statsNode = node.Element(nameof(Stats))!;
            SmallCaliber = GetStatsForWeapon(statsNode.Element(nameof(SmallCaliber)), logger);
            MediumCaliber = GetStatsForWeapon(statsNode.Element(nameof(MediumCaliber)), logger);
            HeavyCaliber = GetStatsForWeapon(statsNode.Element(nameof(HeavyCaliber)), logger);
            LightImpact = GetStatsForWeapon(statsNode.Element(nameof(LightImpact)), logger);
            HeavyImpact = GetStatsForWeapon(statsNode.Element(nameof(HeavyImpact)), logger);
            Shotgun = GetStatsForWeapon(statsNode.Element(nameof(Shotgun)), logger);
            Cutting = GetStatsForWeapon(statsNode.Element(nameof(Cutting)), logger);

            AllStats = new[] {
                SmallCaliber,
                MediumCaliber,
                HeavyCaliber,
                LightImpact,
                HeavyImpact,
                Shotgun,
                Cutting,
            };

            SuggestWeapons(logger);
        }

        private void SuggestWeapons(ILogger logger) {
            HashSet<uint>[] allHashSets = AllStats
                                          .Select(x => x.Hashes)
                                          .Append(IgnoreHashes)
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
            var type = (PedHitData.WeaponTypes)Enum.Parse(typeof(PedHitData.WeaponTypes), name);
            HashSet<uint> hashes = ExtractWeaponHashes(weaponNode, logger);

            return new Stats(type,
                             hashes,
                             weaponNode.GetFloat(nameof(Stats.DamageMult)),
                             weaponNode.GetFloat(nameof(Stats.BleedMult)),
                             weaponNode.GetFloat(nameof(Stats.PainMult)),
                             weaponNode.GetFloat(nameof(Stats.CritChance)),
                             weaponNode.GetInt(nameof(Stats.ArmorDamage)),
                             weaponNode.GetBool(nameof(Stats.CanPenetrateArmor)),
                             weaponNode.GetFloat(nameof(Stats.HelmetSafeChance)));
        }

        private static HashSet<uint> ExtractWeaponHashes(XElement node, ILogger logger) {
            const string name = "Hashes";
            string hashesString = node.Element(name)!.Attribute(name)!.Value;
            string[] splitStrings = hashesString.Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries);
            var set = new HashSet<uint>(splitStrings.Length);
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

            return set;
        }

        public static string BuildWeaponName(uint hash) {
            string name = Weapon.GetHumanNameFromHash((WeaponHash)hash);
            return $"{name}({hash.ToString()})";
        }
    }
}