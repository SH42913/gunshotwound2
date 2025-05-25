namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;
    using GTA;
    using HitDetection;
    using PedsFeature;
    using Utils;

    public sealed class ArmorChecker {
        private readonly SharedData sharedData;

        public ArmorChecker(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public bool TrySave(ref ConvertedPed convertedPed, in PedHitData hit, in WeaponConfig.Weapon weapon, out WoundData? armorWound) {
            Ped ped = convertedPed.thisPed;
            switch (hit.bodyPart.Key) {
                case "Head":
                    bool hasHelmet = ped.IsWearingHelmet || sharedData.mainConfig.armorConfig.PedHasHelmet(ped);
                    if (hasHelmet && sharedData.random.IsTrueWithProbability(weapon.HelmetSafeChance)) {
                        ShowArmorMessage(convertedPed, sharedData.localeConfig.HelmetSavedYourHead);
                        armorWound = null;
                        return true;
                    } else {
                        armorWound = null;
                        return false;
                    }

                case "Chest":
                case "Abdomen":
                    break;
                default:
                    armorWound = null;
                    return false;
            }

            if (CheckArmorPenetration(ped, weapon, out string reason)) {
                ShowArmorMessage(convertedPed, reason);
                armorWound = null;
                return false;
            }

            switch (hit.bodyPart.Key) {
                case "Chest":
                    ShowArmorMessage(convertedPed, sharedData.localeConfig.ArmorSavedYourChest);
                    armorWound = GetUnderArmorWound(weapon.ArmorDamage, damageMult: 1f, painMult: 2f);
                    return true;
                case "Abdomen":
                    ShowArmorMessage(convertedPed, sharedData.localeConfig.ArmorSavedYourLowerBody);
                    armorWound = GetUnderArmorWound(weapon.ArmorDamage, damageMult: 1f, painMult: 3f);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private bool CheckArmorPenetration(Ped ped, WeaponConfig.Weapon weaponWeapon, out string reason) {
            if (ped.Armor < 1) {
#if DEBUG
                sharedData.logger.WriteInfo("Has no armor");
#endif
                reason = null;
                return true;
            }

            ped.Armor -= weaponWeapon.ArmorDamage;

            ArmorConfig armorConfig = sharedData.mainConfig.armorConfig;
            if (!armorConfig.TryGetArmorLevel(ped, out ArmorConfig.Level armorLevel)) {
#if DEBUG
                sharedData.logger.WriteInfo("Armor is dead");
#endif
                reason = sharedData.localeConfig.ArmorDestroyed;
                return true;
            }

            if (!weaponWeapon.CanPenetrateArmor) {
#if DEBUG
                sharedData.logger.WriteInfo("Can't penetrate armor");
#endif
                reason = null;
                return false;
            }

            float chanceToSave = armorConfig.MinimalChanceForArmorSave;
            if (chanceToSave >= 1f) {
                sharedData.logger.WriteWarning("Minimal chance for armor save is >1");
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

        private WoundData? GetUnderArmorWound(float armorDamage, float damageMult, float painMult) {
            if (armorDamage < 1) {
                return null;
            }

            return new WoundData {
                Name = sharedData.localeConfig.ArmorInjury,
                Damage = damageMult * armorDamage,
                Pain = painMult * armorDamage,
            };
        }

        private void ShowArmorMessage(in ConvertedPed convertedPed, string message) {
            if (convertedPed.isPlayer) {
                sharedData.notifier.critical.QueueMessage(message, Notifier.Color.YELLOW);
            }
        }
    }
}