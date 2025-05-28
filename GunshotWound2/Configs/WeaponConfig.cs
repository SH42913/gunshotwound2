// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
    using SHVDN;
    using Utils;

    public sealed class WeaponConfig : MainConfig.IConfig {
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

        public string sectionName => "Weapons.xml";

        private readonly List<string> invalidWeapons = new();

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(WeaponConfig))!;
            UseSpecialStunDamage = root.Element(nameof(UseSpecialStunDamage)).GetBool();
            IgnoreSet = ExtractWeaponHashes(root.Element(nameof(IgnoreSet)));

            XElement weaponsNode = root.Element(nameof(Weapons))!;
            Weapons = weaponsNode.Elements().Select(GetStatsForWeapon).ToArray();

            const string minSpeedName = "MinSpeed";
            LightFallThreshold = weaponsNode.Element(nameof(LightFall)).GetFloat(minSpeedName);
            HardFallThreshold = weaponsNode.Element(nameof(HardFall)).GetFloat(minSpeedName);
            LightRunOverThreshold = weaponsNode.Element(nameof(LightRunOverCar)).GetFloat(minSpeedName);
            HardRunOverThreshold = weaponsNode.Element(nameof(HardRunOverCar)).GetFloat(minSpeedName);

            const string prefix = "";
            WEAPON_FALL = GetWeaponHash(nameof(WEAPON_FALL), prefix);
            WEAPON_EXHAUSTION = GetWeaponHash(nameof(WEAPON_EXHAUSTION), prefix);
            WEAPON_FIRE = GetWeaponHash(nameof(WEAPON_FIRE), prefix);
            WEAPON_RAMMED_BY_CAR = GetWeaponHash(nameof(WEAPON_RAMMED_BY_CAR), prefix);
            WEAPON_RUN_OVER_BY_CAR = GetWeaponHash(nameof(WEAPON_RUN_OVER_BY_CAR), prefix);
        }

        public void Validate(MainConfig mainConfig, ILogger logger) {
            foreach (string invalidWeapon in invalidWeapons) {
                logger.WriteWarning($"{invalidWeapon} is not valid weapon. GSW2 will ignore it.");
            }

            foreach (Weapon weapon in Weapons) {
                foreach ((string key, int _) in weapon.Wounds) {
                    if (!mainConfig.woundConfig.Wounds.ContainsKey(key)) {
                        logger.WriteWarning($"{weapon.Key} has invalid wound {key}");
                    }
                }

                bool isValidArmor = string.IsNullOrEmpty(weapon.SafeArmorLevel)
                                    || Array.Exists(mainConfig.armorConfig.Levels, x => x.Key == weapon.SafeArmorLevel);

                if (!isValidArmor) {
                    logger.WriteWarning($"{weapon.Key} has invalid armor level {weapon.SafeArmorLevel}");
                }
            }

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

        private Weapon GetStatsForWeapon(XElement weaponNode) {
            string key = weaponNode.Name.LocalName;
            HashSet<uint> hashes = ExtractWeaponHashes(weaponNode);
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

        private HashSet<uint> ExtractWeaponHashes(XElement node) {
            var set = new HashSet<uint>();
            foreach (XElement element in node.Elements()) {
                const string attributeName = "Hashes";
                string attributeValue = element.GetString(attributeName);
                if (string.IsNullOrEmpty(attributeValue)) {
                    continue;
                }

                string[] splitStrings = attributeValue.Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string weaponString in splitStrings) {
                    uint hash = GetWeaponHash(weaponString);
                    if (NativeMemory.IsHashValidAsWeaponHash(hash)) {
                        set.Add(hash);
                    } else {
                        invalidWeapons.Add(weaponString);
                    }
                }
            }

            return set;
        }

        private static uint GetWeaponHash(string weaponString, string prefix = "WEAPON_") {
            if (!uint.TryParse(weaponString, out uint hash)) {
                hash = StringHash.AtStringHashUtf8(prefix + weaponString.ToUpper());
            }

            return hash;
        }

        public static string BuildWeaponName(uint hash) {
            string name = GTA.Weapon.GetHumanNameFromHash((WeaponHash)hash);
            return $"{name}({hash.ToString()})";
        }
    }
}