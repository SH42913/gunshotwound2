namespace GunshotWound2.WoundFeature {
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using HitDetection;
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

            Dictionary<string, float?[]> damageSystemConfigs = sharedData.mainConfig.WoundConfig.DamageSystemConfigs;
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

        public WoundData? ProcessHit(Ped ped, in PedHitData hit) {
            if (hit.bodyPart == PedHitData.BodyParts.Nothing || hit.weaponType == PedHitData.WeaponTypes.Nothing) {
                sharedData.logger.WriteWarning("Hit is invalid, will be used default wound");
                return DefaultWound();
            }

            if (hit.bodyPart == PedHitData.BodyParts.Head
                && ped.IsWearingHelmet
                && sharedData.random.IsTrueWithProbability(HelmetSafeChance)) {
                return GetUnderArmorWound(damageMult: 3f, painMult: 5f, sharedData.localeConfig.HelmetSavedYourHead);
            }

            if (hit.bodyPart == PedHitData.BodyParts.UpperBody && !CheckArmorPenetration(ped)) {
                return GetUnderArmorWound(damageMult: 1f, painMult: 2f, sharedData.localeConfig.ArmorSavedYourChest);
            }

            if (hit.bodyPart == PedHitData.BodyParts.LowerBody && !CheckArmorPenetration(ped)) {
                return GetUnderArmorWound(damageMult: 2f, painMult: 3f, sharedData.localeConfig.ArmorSavedYourLowerBody);
            }

            switch (hit.bodyPart) {
                case PedHitData.BodyParts.Head:      return GetHeadWound();
                case PedHitData.BodyParts.Neck:      return GetNeckWound();
                case PedHitData.BodyParts.UpperBody: return GetUpperWound();
                case PedHitData.BodyParts.LowerBody: return GetLowerWound();
                case PedHitData.BodyParts.Arm:       return GetArmWound();
                case PedHitData.BodyParts.Leg:       return GetLegWound();
                default:                             return null;
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
            return CreateWound(name, 50f, 4f, 130f);
        }

        protected WoundData CreateWound(string name,
                                        float damage,
                                        float bleeding,
                                        float pain,
                                        float arteryDamageChance = -1,
                                        bool hasCrits = false) {
            return new WoundData {
                Name = name,
                Damage = damageMultiplier * damage,
                Pain = painMultiplier * pain,
                BleedSeverity = bleedingMultiplier * bleeding,
                ArterySevered = arteryDamageChance > 0 && sharedData.random.IsTrueWithProbability(arteryDamageChance),
                HasCrits = hasCrits && sharedData.random.IsTrueWithProbability(critChance),
            };
        }

        private bool CheckArmorPenetration(Ped ped) {
            if (ped.Armor <= 0) {
#if DEBUG
                sharedData.logger.WriteInfo("Has no armor");
#endif
                return true;
            }

            ped.Armor -= armorDamage;
            if (ped.Armor <= 0) {
#if DEBUG
                sharedData.logger.WriteInfo("Armor is dead");
#endif
                sharedData.notifier.alert.AddMessage(sharedData.localeConfig.ArmorDestroyed);
                return true;
            }

            if (!CanPenetrateArmor) {
#if DEBUG
                sharedData.logger.WriteInfo("Can't penetrate armor");
#endif
                return false;
            }

            WoundConfig woundConfig = sharedData.mainConfig.WoundConfig;
            float chanceToSave = woundConfig.MinimalChanceForArmorSave;
            if (chanceToSave >= 1f) {
#if DEBUG
                sharedData.logger.WriteInfo("Minimal chance for armor save is >1");
#endif
                return false;
            }

            float armorPercent = ped.Armor / 100f;
            float chanceForArmorPercent = 1f - woundConfig.MinimalChanceForArmorSave;
            float saveProbability = woundConfig.MinimalChanceForArmorSave + chanceForArmorPercent * armorPercent;
            if (sharedData.random.IsTrueWithProbability(saveProbability)) {
#if DEBUG
                sharedData.logger.WriteInfo($"Armor deflected damage, save probability {saveProbability.ToString("F2")}");
#endif
                return false;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Armor penetrated, save probability {saveProbability.ToString("F2")}");
#endif
            sharedData.notifier.warning.AddMessage(sharedData.localeConfig.ArmorPenetrated);
            return true;
        }

        private WoundData GetUnderArmorWound(float damageMult, float painMult, string reason) {
            return new WoundData {
                Name = sharedData.localeConfig.ArmorInjury,
                AdditionalMessage = reason,
                Damage = damageMult * armorDamage,
                Pain = painMult * armorDamage,
            };
        }
    }
}