namespace GunshotWound2.WoundFeature {
    using Configs;
    using GTA;
    using HitDetection;
    using PedsFeature;
    using Utils;

    public sealed class ArmorChecker {
        private static readonly WoundConfig.Wound ARMOR_INJURY = new(key: "ArmorInjury", locKey: "ArmorBluntTrauma", isBlunt: true);

        private readonly SharedData sharedData;

        public ArmorChecker(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public bool TrySave(ref ConvertedPed convertedPed,
                            in PedHitData hit,
                            out WoundConfig.Wound armorWound) {
            if (!hit.bodyPart.IsValid) {
                armorWound = default;
                return false;
            }

            if (hit.bodyPart.Bones.Contains((int)Bone.SkelHead)) {
                armorWound = default;
                return CheckHelmetPenetration(convertedPed, hit.weaponType);
            }

            Ped ped = convertedPed.thisPed;
            ArmorConfig armorConfig = sharedData.mainConfig.armorConfig;
            if (!armorConfig.TryGetArmorLevel(ped.Armor, out ArmorConfig.Level armorLevel)) {
#if DEBUG
                sharedData.logger.WriteInfo("Has no armor");
#endif
                armorWound = default;
                return false;
            }

            bool hitArmor = armorLevel.Parts.Contains(hit.bodyPart.Key);
            if (!hitArmor) {
#if DEBUG
                sharedData.logger.WriteInfo("Was not touch armor");
#endif
                armorWound = default;
                return false;
            }

            int armorDamage = hit.weaponType.ArmorDamage;
            ped.Armor -= armorDamage;
            LocaleConfig localeConfig = sharedData.localeConfig;
            if (!armorConfig.TryGetArmorLevel(ped.Armor, out armorLevel)) {
#if DEBUG
                sharedData.logger.WriteInfo("Armor is dead");
#endif
                ShowArmorMessage(convertedPed, localeConfig.ArmorDestroyed);
                armorWound = default;
                return false;
            }

            if (CheckArmorPenetration(ped, hit.weaponType)) {
                ShowArmorMessage(convertedPed, localeConfig.ArmorPenetrated);
                armorWound = default;
                return false;
            }

            ShowArmorMessage(convertedPed, localeConfig.ArmorProtectedYou);

            if (armorDamage > 0 && armorLevel.TraumaPadEfficiency < 1f) {
                float traumaPadAbsorbed = armorDamage * armorLevel.TraumaPadEfficiency;
                float bluntDamage = armorDamage - traumaPadAbsorbed;
                bool canCauseTrauma = armorLevel.MaxValue / bluntDamage < 10;
                armorWound = ARMOR_INJURY.Clone(damage: bluntDamage, pain: bluntDamage, canCauseTrauma: canCauseTrauma);
            } else {
                armorWound = default;
            }

            return true;
        }

        private bool CheckHelmetPenetration(in ConvertedPed convertedPed, in WeaponConfig.Weapon weapon) {
            Ped ped = convertedPed.thisPed;
            bool hasHelmet = ped.IsWearingHelmet || sharedData.mainConfig.armorConfig.PedHasHelmet(ped);
            if (hasHelmet && sharedData.random.IsTrueWithProbability(weapon.HelmetSafeChance)) {
                ShowArmorMessage(convertedPed, sharedData.localeConfig.HelmetProtectedYou);
                return true;
            } else {
                return false;
            }
        }

        private bool CheckArmorPenetration(Ped ped, in WeaponConfig.Weapon weapon) {
            ArmorConfig armorConfig = sharedData.mainConfig.armorConfig;
            if (string.IsNullOrEmpty(weapon.SafeArmorLevel)) {
#if DEBUG
                sharedData.logger.WriteInfo("Can't penetrate armor");
#endif
                return false;
            }

            float chanceToSave = armorConfig.MinimalChanceForArmorSave;
            if (chanceToSave >= 1f) {
                sharedData.logger.WriteWarning("Minimal chance for armor save is >1");
                return false;
            }

            ArmorConfig.Level weaponSafeLevel = sharedData.mainConfig.armorConfig.GetArmorLevelByKey(weapon.SafeArmorLevel);
            float armorPercent = (float)ped.Armor / weaponSafeLevel.MaxValue;
            if (armorPercent >= 1f) {
#if DEBUG
                sharedData.logger.WriteInfo($"{weapon.Key} can't penetrate safe armor level {weapon.SafeArmorLevel}({ped.Armor})");
#endif
                return false;
            }

            float chanceForArmorPercent = 1f - armorConfig.MinimalChanceForArmorSave;
            float saveProbability = armorConfig.MinimalChanceForArmorSave + chanceForArmorPercent * armorPercent;
            if (sharedData.random.IsTrueWithProbability(saveProbability)) {
#if DEBUG
                sharedData.logger.WriteInfo($"Armor deflected damage, save probability {saveProbability.ToString("F2")}");
#endif
                return false;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Armor penetrated, save probability {saveProbability.ToString("F2")}");
#endif
            return true;
        }

        private void ShowArmorMessage(in ConvertedPed convertedPed, string message) {
            if (convertedPed.isPlayer) {
                sharedData.notifier.critical.QueueMessage(message, Notifier.Color.YELLOW);
            }
        }
    }
}