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
        public readonly struct Weapon {
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

            public Weapon(string key,
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

        public const uint WEAPON_FALL = 3452007600;
        public const uint WEAPON_EXHAUSTION = 910830060;
        public const uint WEAPON_FIRE = 3750660587;
        public const uint WEAPON_RAMMED_BY_CAR = 133987706;
        public const uint WEAPON_RUN_OVER_BY_CAR = 2741846334;

        public bool UseSpecialStunDamage;
        public HashSet<uint> IgnoreSet;
        public Weapon[] Weapons;

        public float LightFallThreshold;
        public float HardFallThreshold;

        public float LightRunOverThreshold;
        public float HardRunOverThreshold;

        public Weapon Stun => Weapons.First(x => x.Key == nameof(Stun));
        public Weapon Takedown => Weapons.First(x => x.Key == nameof(Takedown));
        public Weapon LightFall => Weapons.First(x => x.Key == nameof(LightFall));
        public Weapon HardFall => Weapons.First(x => x.Key == nameof(HardFall));
        public Weapon LightRunOverCar => Weapons.First(x => x.Key == nameof(LightRunOverCar));
        public Weapon HardRunOverCar => Weapons.First(x => x.Key == nameof(HardRunOverCar));
        public Weapon CarCrash => Weapons.First(x => x.Key == nameof(CarCrash));

        public void FillFrom(XDocument doc, ILogger logger) {
            XElement node = doc.Element(nameof(WeaponConfig))!;
            UseSpecialStunDamage = node.Element(nameof(UseSpecialStunDamage)).GetBool();
            IgnoreSet = ExtractWeaponHashes(node.Element(nameof(IgnoreSet)), logger);

            XElement weaponsNode = node.Element(nameof(Weapons))!;
            Weapons = weaponsNode.Elements().Select(x => GetStatsForWeapon(x, logger)).ToArray();

            const string minSpeedName = "MinSpeed";
            LightFallThreshold = weaponsNode.Element(nameof(LightFall)).GetFloat(minSpeedName);
            HardFallThreshold = weaponsNode.Element(nameof(HardFall)).GetFloat(minSpeedName);
            LightRunOverThreshold = weaponsNode.Element(nameof(LightRunOverCar)).GetFloat(minSpeedName);
            HardRunOverThreshold = weaponsNode.Element(nameof(HardRunOverCar)).GetFloat(minSpeedName);

            SuggestWeapons(logger);
        }

        private void SuggestWeapons(ILogger logger) {
            HashSet<uint>[] allHashSets = Weapons
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

        private static Weapon GetStatsForWeapon(XElement weaponNode, ILogger logger) {
            string name = weaponNode.Name.LocalName;
            HashSet<uint> hashes = ExtractWeaponHashes(weaponNode, logger);
            (string, int)[] wounds = ExtractWounds(weaponNode);

            return new Weapon(name,
                              hashes,
                              wounds,
                              weaponNode.GetFloat(nameof(Weapon.DamageMult), defaultValue: 1f),
                              weaponNode.GetFloat(nameof(Weapon.BleedMult), defaultValue: 1f),
                              weaponNode.GetFloat(nameof(Weapon.PainMult), defaultValue: 1f),
                              weaponNode.GetFloat(nameof(Weapon.CritChance), defaultValue: 0f),
                              weaponNode.GetInt(nameof(Weapon.ArmorDamage), defaultValue: 0),
                              weaponNode.GetBool(nameof(Weapon.CanPenetrateArmor), defaultValue: false),
                              weaponNode.GetFloat(nameof(Weapon.HelmetSafeChance), defaultValue: 0f));
        }

        private static (string, int)[] ExtractWounds(XElement weaponNode) {
            XElement woundsNode = weaponNode.Element(nameof(Weapon.Wounds))!;
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
            string name = GTA.Weapon.GetHumanNameFromHash((WeaponHash)hash);
            return $"{name}({hash.ToString()})";
        }
    }
}