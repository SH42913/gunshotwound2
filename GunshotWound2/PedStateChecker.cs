namespace GunshotWound2 {
    using System;
    using System.Text;
    using Configs;
    using CritsFeature;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public static class PedStateChecker {
        private static readonly StringBuilder STRING_BUILDER = new();
        private static GTA.FeedPost LAST_POST;

        public static void Check(SharedData sharedData, Entity pedEntity) {
            ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
            ref Health health = ref pedEntity.GetComponent<Health>();

            STRING_BUILDER.Clear();
            ShowHealth(sharedData, ref convertedPed, ref health);
            ShowPain(sharedData, pedEntity);
            ShowCrits(sharedData, pedEntity);
            ShowArmor(sharedData, ref convertedPed);
            ShowBleedingWounds(sharedData, ref convertedPed, ref health);

            LAST_POST?.Delete();
            LAST_POST = GTA.UI.Notification.PostTicker(STRING_BUILDER.ToString(), isImportant: true);
        }

        private static void ShowHealth(SharedData sharedData, ref ConvertedPed convertedPed, ref Health health) {
            int currentHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
            int maxHealth = WoundConfig.ConvertHealthFromNative(health.max);
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent <= 0f) {
                STRING_BUILDER.AppendLine($"~r~{sharedData.localeConfig.YouAreDead}~s~");
                return;
            }

            string color;
            if (healthPercent >= 0.75f) {
                color = "~g~";
            } else if (healthPercent > 0.5f) {
                color = "~y~";
            } else if (healthPercent > 0.25f) {
                color = "~o~";
            } else {
                color = "~r~";
            }

            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.Append(sharedData.localeConfig.Health);
            STRING_BUILDER.Append(": ");
            STRING_BUILDER.Append(color);
            STRING_BUILDER.Append(healthPercent.ToString("P0"));
            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.AppendEndOfLine();
        }

        private static void ShowArmor(SharedData sharedData, ref ConvertedPed convertedPed) {
            int armor = convertedPed.thisPed.Armor;
            if (armor <= 0) {
                return;
            }

            string color;
            string message;
            if (armor >= 60) {
                color = "~g~";
                message = sharedData.localeConfig.ArmorLooksGreat;
            } else if (armor >= 40) {
                color = "~y~";
                message = sharedData.localeConfig.ScratchesOnArmor;
            } else if (armor >= 20) {
                color = "~o~";
                message = sharedData.localeConfig.DentsOnArmor;
            } else {
                color = "~r~";
                message = sharedData.localeConfig.ArmorLooksAwful;
            }

            STRING_BUILDER.Append(color);
            STRING_BUILDER.Append(message);
#if DEBUG
            STRING_BUILDER.Append($" ({armor.ToString()})");
#endif
            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.AppendEndOfLine();
        }

        private static void ShowPain(SharedData sharedData, Entity pedEntity) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.currentState == null) {
                return;
            }

            float painPercent = pain.Percent();
            STRING_BUILDER.Append(sharedData.localeConfig.Pain);
            STRING_BUILDER.Append(": ");
            STRING_BUILDER.Append(pain.currentState?.Color ?? "~g~");
            STRING_BUILDER.Append(painPercent.ToString("P1"));

            if (painPercent > 1f) {
                float timeToRecover = (pain.amount - pain.max) / pain.recoveryRate;
                STRING_BUILDER.Append($" ({timeToRecover.ToString("F1")} sec)");
            }

            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.AppendEndOfLine();
        }

        private static void ShowBleedingWounds(SharedData sharedData, ref ConvertedPed convertedPed, ref Health health) {
            if (health.bleedingWounds == null || health.bleedingWounds.Count < 1) {
                return;
            }

            LocaleConfig localeConfig = sharedData.localeConfig;
            STRING_BUILDER.AppendEndOfLine();
            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.Append(localeConfig.Wounds);
            STRING_BUILDER.Append(':');

            foreach (Entity bleedingEntity in health.bleedingWounds) {
                ref Bleeding bleeding = ref bleedingEntity.GetComponent<Bleeding>();
                bool isBleedingToBandage = health.bleedingToBandage == bleedingEntity;

                STRING_BUILDER.AppendEndOfLine();
                if (isBleedingToBandage) {
                    STRING_BUILDER.Append("~g~-> ");
                }

                int totalHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
                float deadlyRate = totalHealth * health.bleedingHealRate;
                float deadlyBleedingThreshold = (float)Math.Sqrt(deadlyRate);

                string color;
                if (bleeding.severity > deadlyBleedingThreshold) {
                    color = "~r~";
                } else if (bleeding.severity > 0.5f * deadlyBleedingThreshold) {
                    color = "~o~";
                } else if (bleeding.severity > 0.25f * deadlyBleedingThreshold) {
                    color = "~y~";
                } else {
                    color = "~s~";
                }

                STRING_BUILDER.Append(color);
                STRING_BUILDER.Append(bleeding.name);

                if (isBleedingToBandage && health.timeToBandage > 0f) {
                    float bandagingProgress = 1f - (health.timeToBandage / sharedData.mainConfig.WoundConfig.ApplyBandageTime);
                    STRING_BUILDER.Append($" ~g~({bandagingProgress.ToString("P1")})");
                }
            }

            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.AppendEndOfLine();
        }

        private static void ShowCrits(SharedData sharedData, Entity pedEntity) {
            LocaleConfig localeConfig = sharedData.localeConfig;
            ref Crits crits = ref pedEntity.GetComponent<Crits>();
            if (crits.active == Crits.Types.Nothing) {
                return;
            }

            STRING_BUILDER.Append(localeConfig.Crits);
            STRING_BUILDER.Append(" ~r~");

            if (crits.HasActive(Crits.Types.SpineDamaged)) {
                STRING_BUILDER.Append(localeConfig.NervesCrit);
                STRING_BUILDER.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.HeartDamaged)) {
                STRING_BUILDER.Append(localeConfig.HeartCrit);
                STRING_BUILDER.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.LungsDamaged)) {
                STRING_BUILDER.Append(localeConfig.LungsCrit);
                STRING_BUILDER.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.StomachDamaged)) {
                STRING_BUILDER.Append(localeConfig.StomachCrit);
                STRING_BUILDER.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.GutsDamaged)) {
                STRING_BUILDER.Append(localeConfig.GutsCrit);
                STRING_BUILDER.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.ArmsDamaged)) {
                STRING_BUILDER.Append(localeConfig.ArmsCrit);
                STRING_BUILDER.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.LegsDamaged)) {
                STRING_BUILDER.Append(localeConfig.LegsCrit);
                STRING_BUILDER.AppendSpace();
            }

            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.AppendEndOfLine();
        }
    }
}