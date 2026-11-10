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

            if (hit.bodyPart.IsHead()) {
                armorWound = default;
                return CheckHelmetPenetration(convertedPed, hit.weaponType);
            }

            Ped ped = convertedPed.thisPed;
            ArmorConfig armorConfig = sharedData.mainConfig.armorConfig;
            int armorBefore = ped.Armor;
            if (!armorConfig.TryGetArmorLevel(armorBefore, out ArmorConfig.Level armorLevel)) {
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

            bool penetrated = CheckArmorPenetration(armorBefore, hit.weaponType);
            int armorDamage = hit.weaponType.ArmorDamage * hit.hits;
            int newArmor = armorBefore - armorDamage;
            convertedPed.SetArmor(newArmor);

            LocaleConfig localeConfig = sharedData.localeConfig;
            if (penetrated) {
                string message = newArmor <= 0 ? localeConfig.ArmorDestroyed : localeConfig.ArmorPenetrated;
                ShowArmorMessage(convertedPed, message);
                armorWound = default;
                return false;
            }

            ShowArmorMessage(convertedPed, localeConfig.ArmorProtectedYou);
            if (newArmor <= 0) {
                ShowArmorMessage(convertedPed, localeConfig.ArmorDestroyed);
            }

            if (armorDamage > 0 && armorLevel.TraumaPadEfficiency < 1f) {
                float traumaPadAbsorbed = armorDamage * armorLevel.TraumaPadEfficiency;
                float bluntDamage = armorDamage - traumaPadAbsorbed;
                float chanceToCauseTrauma = hit.weaponType.ChanceToCauseTrauma * (1f - armorLevel.TraumaPadEfficiency);
                bool canCauseTrauma = sharedData.random.IsTrueWithProbability(chanceToCauseTrauma);
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

        private bool CheckArmorPenetration(int armorBefore, in WeaponConfig.Weapon weapon) {
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

            ArmorConfig.Level weaponSafeLevel = armorConfig.GetArmorLevelByKey(weapon.SafeArmorLevel);
            float saveProbability = armorConfig.GetArmorSaveProbability(weaponSafeLevel, armorBefore);
            if (saveProbability >= 1f) {
#if DEBUG
                sharedData.logger.WriteInfo($"{weapon.Key} can't penetrate safe armor level {weapon.SafeArmorLevel}");

                string armorKey = armorConfig.TryGetArmorLevel(armorBefore, out ArmorConfig.Level level) ? level.Key : "NO ARMOR";
                sharedData.logger.WriteInfo($"Target has {armorKey}({armorBefore}) armor");
#endif
                return false;
            }

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