namespace GunshotWound2.WoundFeature {
    using System;
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using HitDetection;
    using PedsFeature;
    using Utils;

    public abstract class BaseWeaponDamage {
        protected readonly SharedData sharedData;
        protected readonly Weighted_Randomizer.IWeightedRandomizer<int> randomizer;

        private readonly float damageMultiplier;
        private readonly float bleedingMultiplier;
        private readonly float painMultiplier;
        private readonly float critChance;
        private readonly int armorDamage;

        protected abstract bool CanPenetrateArmor { get; }
        protected abstract float HelmetSafeChance { get; }

        protected BaseWeaponDamage(SharedData sharedData, string weaponClass) {
            this.sharedData = sharedData;
            randomizer = sharedData.weightRandom;

            Dictionary<string, float?[]> damageSystemConfigs = sharedData.mainConfig.weaponConfig.DamageSystemConfigs;
            if (damageSystemConfigs != null
                && damageSystemConfigs.TryGetValue(weaponClass, out float?[] multsArray)
                && multsArray.Length == 5) {
                if (multsArray[0].HasValue) {
                    damageMultiplier = multsArray[0].Value;
                }

                if (multsArray[1].HasValue) {
                    bleedingMultiplier = multsArray[1].Value;
                }

                if (multsArray[2].HasValue) {
                    painMultiplier = multsArray[2].Value;
                }

                if (multsArray[3].HasValue) {
                    critChance = multsArray[3].Value;
                }

                if (multsArray[4].HasValue) {
                    armorDamage = (int)multsArray[4].Value;
                }
            }
        }

        public WoundData? ProcessHit(ref ConvertedPed convertedPed, ref PedHitData hit) {
            if (hit.bodyPart == PedHitData.BodyParts.Nothing || hit.weaponType == PedHitData.WeaponTypes.Nothing) {
                sharedData.logger.WriteWarning("Hit is invalid, will be used default wound");
                return DefaultWound();
            }

            if (TrySaveWithArmor(convertedPed.thisPed, ref hit, out WoundData? armorWound)) {
                return armorWound;
            }

            switch (hit.bodyPart) {
                case PedHitData.BodyParts.Head:    return GetHeadWound();
                case PedHitData.BodyParts.Neck:    return GetNeckWound();
                case PedHitData.BodyParts.Chest:   return GetUpperWound();
                case PedHitData.BodyParts.Abdomen: return GetLowerWound();
                case PedHitData.BodyParts.Arm:     return GetArmWound();
                case PedHitData.BodyParts.Leg:     return GetLegWound();
                default:                           return null;
            }
        }

        protected abstract WoundData DefaultWound();
        protected abstract WoundData GetHeadWound();
        protected abstract WoundData GetNeckWound();
        protected abstract WoundData GetUpperWound();
        protected abstract WoundData GetLowerWound();
        protected abstract WoundData GetArmWound();
        protected abstract WoundData GetLegWound();

        protected WoundData CreateHeavyBrainDamage(string name) {
            return CreateWound(name, 50f, 4f, 130f, hasCrits: true, ignoreCritsChance: true, internalBleeding: true);
        }

        protected WoundData CreateWound(string name,
                                        float damage,
                                        float bleeding,
                                        float pain,
                                        float arteryDamageChance = -1,
                                        bool hasCrits = false,
                                        bool ignoreCritsChance = false,
                                        bool internalBleeding = false) {
            return new WoundData {
                Name = name,
                Damage = damageMultiplier * damage,
                Pain = painMultiplier * pain,
                BleedSeverity = bleedingMultiplier * bleeding,
                InternalBleeding = internalBleeding,
                ArterySevered = arteryDamageChance > 0 && sharedData.random.IsTrueWithProbability(arteryDamageChance),
                HasCrits = hasCrits && (ignoreCritsChance || sharedData.random.IsTrueWithProbability(critChance)),
            };
        }

        private bool TrySaveWithArmor(Ped ped, ref PedHitData hit, out WoundData? armorWound) {
            switch (hit.bodyPart) {
                case PedHitData.BodyParts.Head:
                    bool hasHelmet = ped.IsWearingHelmet || sharedData.mainConfig.armorConfig.PedHasHelmet(ped);
                    if (hasHelmet && sharedData.random.IsTrueWithProbability(HelmetSafeChance)) {
                        hit.armorMessage = sharedData.localeConfig.HelmetSavedYourHead;
                        armorWound = null;
                        return true;
                    } else {
                        armorWound = null;
                        return false;
                    }

                case PedHitData.BodyParts.Chest:
                case PedHitData.BodyParts.Abdomen:
                    break;
                default:
                    armorWound = null;
                    return false;
            }

            if (CheckArmorPenetration(ped, out string reason)) {
                hit.armorMessage = reason;
                armorWound = null;
                return false;
            }

            switch (hit.bodyPart) {
                case PedHitData.BodyParts.Chest:
                    hit.armorMessage = sharedData.localeConfig.ArmorSavedYourChest;
                    armorWound = GetUnderArmorWound(damageMult: 1f, painMult: 2f);
                    return true;
                case PedHitData.BodyParts.Abdomen:
                    hit.armorMessage = sharedData.localeConfig.ArmorSavedYourLowerBody;
                    armorWound = GetUnderArmorWound(damageMult: 1f, painMult: 3f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private bool CheckArmorPenetration(Ped ped, out string reason) {
            if (ped.Armor <= 0) {
#if DEBUG
                sharedData.logger.WriteInfo("Has no armor");
#endif
                reason = null;
                return true;
            }

            ped.Armor -= armorDamage;
            if (ped.Armor <= 0) {
#if DEBUG
                sharedData.logger.WriteInfo("Armor is dead");
#endif
                reason = sharedData.localeConfig.ArmorDestroyed;
                return true;
            }

            if (!CanPenetrateArmor) {
#if DEBUG
                sharedData.logger.WriteInfo("Can't penetrate armor");
#endif
                reason = null;
                return false;
            }

            ArmorConfig armorConfig = sharedData.mainConfig.armorConfig;
            float chanceToSave = armorConfig.MinimalChanceForArmorSave;
            if (chanceToSave >= 1f) {
#if DEBUG
                sharedData.logger.WriteInfo("Minimal chance for armor save is >1");
#endif
                reason = null;
                return false;
            }

            float armorPercent = ped.Armor / 100f;
            float chanceForArmorPercent = 1f - armorConfig.MinimalChanceForArmorSave;
            float saveProbability = armorConfig.MinimalChanceForArmorSave + chanceForArmorPercent * armorPercent;
            if (sharedData.random.IsTrueWithProbability(saveProbability)) {
#if DEBUG
                sharedData.logger.WriteInfo($"Armor deflected damage, save probability {saveProbability.ToString("F2")}");
#endif
                reason = null;
                return false;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Armor penetrated, save probability {saveProbability.ToString("F2")}");
#endif
            reason = sharedData.localeConfig.ArmorPenetrated;
            return true;
        }

        private WoundData? GetUnderArmorWound(float damageMult, float painMult) {
            if (armorDamage < 1) {
                return null;
            }

            return new WoundData {
                Name = sharedData.localeConfig.ArmorInjury,
                Damage = damageMult * armorDamage,
                Pain = painMult * armorDamage,
            };
        }
    }
}