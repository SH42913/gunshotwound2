// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using SHVDN;
    using Utils;

    public sealed class WeaponConfig {
        public readonly struct Weapon {
            public readonly string Key;
            public readonly string ShortDesc;
            public readonly HashSet<uint> Hashes;
            public readonly (string key, int weight)[] Wounds;
            public readonly float DamageMult;
            public readonly float BleedMult;
            public readonly float PainMult;
            public readonly float ChanceToCauseTrauma;
            public readonly float HelmetSafeChance;
            public readonly string SafeArmorLevel;
            public readonly int ArmorDamage;

            public bool IsValid => !string.IsNullOrEmpty(Key);

            public Weapon(string key,
                          string shortDesc,
                          HashSet<uint> hashes,
                          (string key, int weight)[] wounds,
                          float damageMult,
                          float bleedMult,
                          float painMult,
                          float chanceToCauseTrauma,
                          float helmetSafeChance,
                          string safeArmorLevel,
                          int armorDamage) {
                Key = key;
                ShortDesc = shortDesc;
                Hashes = hashes;
                Wounds = wounds;
                DamageMult = damageMult;
                BleedMult = bleedMult;
                PainMult = painMult;
                ChanceToCauseTrauma = chanceToCauseTrauma;
                HelmetSafeChance = helmetSafeChance;
                SafeArmorLevel = safeArmorLevel;
                ArmorDamage = armorDamage;
            }
        }

        public uint WEAPON_FALL;
        public uint WEAPON_EXHAUSTION;
        public uint WEAPON_FIRE;
        public uint WEAPON_RAMMED_BY_CAR;
        public uint WEAPON_RUN_OVER_BY_CAR;

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

            const string prefix = "";
            TryGetWeaponHash(nameof(WEAPON_FALL), out WEAPON_FALL, prefix);
            TryGetWeaponHash(nameof(WEAPON_EXHAUSTION), out WEAPON_EXHAUSTION, prefix);
            TryGetWeaponHash(nameof(WEAPON_FIRE), out WEAPON_FIRE, prefix);
            TryGetWeaponHash(nameof(WEAPON_RAMMED_BY_CAR), out WEAPON_RAMMED_BY_CAR, prefix);
            TryGetWeaponHash(nameof(WEAPON_RUN_OVER_BY_CAR), out WEAPON_RUN_OVER_BY_CAR, prefix);

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
            string key = weaponNode.Name.LocalName;
            HashSet<uint> hashes = ExtractWeaponHashes(weaponNode, logger);
            (string, int)[] wounds = ExtractWounds(weaponNode);

            string shortDesc = weaponNode.GetString(nameof(Weapon.ShortDesc));
            float damageMult = weaponNode.GetFloat(nameof(Weapon.DamageMult), defaultValue: 1f);
            float bleedMult = weaponNode.GetFloat(nameof(Weapon.BleedMult), defaultValue: 1f);
            float painMult = weaponNode.GetFloat(nameof(Weapon.PainMult), defaultValue: 1f);
            float traumaChance = weaponNode.GetFloat(nameof(Weapon.ChanceToCauseTrauma), defaultValue: 0f);
            float helmetChance = weaponNode.GetFloat(nameof(Weapon.HelmetSafeChance), defaultValue: 0f);
            string safeArmorLevel = weaponNode.GetString(nameof(Weapon.SafeArmorLevel));
            int armorDamage = weaponNode.GetInt(nameof(Weapon.ArmorDamage), defaultValue: 0);
            return new Weapon(key,
                              shortDesc,
                              hashes,
                              wounds,
                              damageMult,
                              bleedMult,
                              painMult,
                              traumaChance,
                              helmetChance,
                              safeArmorLevel,
                              armorDamage);
        }

        private static (string, int)[] ExtractWounds(XElement weaponNode) {
            XElement woundsNode = weaponNode.Element(nameof(Weapon.Wounds))!;
            return woundsNode.Elements(nameof(WoundConfig.Wound))
                             .Select(x => (x.GetString(nameof(WoundConfig.Wound.Key)), x.GetInt(MainConfig.WEIGHT_ATTRIBUTE_NAME)))
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
                    if (TryGetWeaponHash(weaponString, out uint hash)) {
                        set.Add(hash);
                    } else {
                        logger.WriteWarning($"{weaponString}({hash.ToString()}) is not valid weapon. GSW2 will ignore it.");
                    }
                }
            }
        }

        private static bool TryGetWeaponHash(string weaponString, out uint hash, string prefix = "WEAPON_") {
            if (!uint.TryParse(weaponString, out hash)) {
                hash = StringHash.AtStringHashUtf8(prefix + weaponString.ToUpper());
            }

            return NativeMemory.IsHashValidAsWeaponHash(hash);
        }

        public static string BuildWeaponName(uint hash) {
            string name = GTA.Weapon.GetHumanNameFromHash((WeaponHash)hash);
            return $"{name}({hash.ToString()})";
        }
    }
}