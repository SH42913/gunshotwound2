namespace GunshotWound2 {
    using System;
    using System.Text;
    using Configs;
    using HealthCare;
    using PainFeature;
    using Peds;
    using Scellecs.Morpeh;
    using Utils;

    public static class PedStateChecker {
        private static readonly StringBuilder STRING_BUILDER = new();

        public static void Check(SharedData sharedData, Entity pedEntity) {
            ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
            ref Health health = ref pedEntity.GetComponent<Health>();

            ShowHealth(sharedData, ref convertedPed, ref health);
            ShowArmor(sharedData, ref convertedPed);
            ShowPain(sharedData, pedEntity);
            // ShowCrits(woundedPed);
            ShowBleedingWounds(sharedData, ref convertedPed, ref health);
        }

        private static void ShowHealth(SharedData sharedData, ref ConvertedPed convertedPed, ref Health health) {
            int currentHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
            int maxHealth = WoundConfig.ConvertHealthFromNative(health.max);
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent <= 0f) {
                sharedData.notifier.info.AddMessage($"~r~{sharedData.localeConfig.YouAreDead}~s~");
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

            STRING_BUILDER.Clear();
            STRING_BUILDER.SetDefaultColor();
            STRING_BUILDER.Append(sharedData.localeConfig.Health);
            STRING_BUILDER.Append(": ");
            STRING_BUILDER.Append(color);
            STRING_BUILDER.Append(healthPercent.ToString("P0"));
            STRING_BUILDER.SetDefaultColor();

            sharedData.notifier.info.AddMessage(STRING_BUILDER.ToString());
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

            STRING_BUILDER.Clear();
            STRING_BUILDER.Append(color);
            STRING_BUILDER.Append(message);
            STRING_BUILDER.SetDefaultColor();

            sharedData.notifier.info.AddMessage(STRING_BUILDER.ToString());
        }

        private static void ShowPain(SharedData sharedData, Entity pedEntity) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.currentState == null) {
                return;
            }

            float painPercent = pain.amount / pain.max;
            STRING_BUILDER.Clear();
            STRING_BUILDER.Append(sharedData.localeConfig.Pain);
            STRING_BUILDER.Append(": ");
            STRING_BUILDER.Append(pain.currentState?.Color ?? "~g~");
            STRING_BUILDER.Append(painPercent.ToString("P1"));

            if (painPercent > 1f) {
                float timeToRecover = (pain.amount - pain.max) / pain.recoveryRate;
                STRING_BUILDER.Append($" ({timeToRecover.ToString("F1")} sec)");
            }
            
            STRING_BUILDER.SetDefaultColor();

            sharedData.notifier.info.AddMessage(STRING_BUILDER.ToString());
        }

        private static void ShowBleedingWounds(SharedData sharedData, ref ConvertedPed convertedPed, ref Health health) {
            if (health.bleedingWounds == null || health.bleedingWounds.Count < 1) {
                return;
            }

            LocaleConfig localeConfig = sharedData.localeConfig;
            STRING_BUILDER.Clear();
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
            sharedData.notifier.info.AddMessage(STRING_BUILDER.ToString());
        }

        //
        // private void ShowCrits(WoundedPedComponent woundedPed)
        // {
        //     var healthInfo = "";
        //     if (woundedPed.Crits <= 0) return;
        //
        //     healthInfo += $"~s~{_locale.Data.Crits} ~r~";
        //
        //     if (woundedPed.Crits.Has(CritTypes.NERVES_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.NervesCrit} ";
        //     }
        //
        //     if (woundedPed.Crits.Has(CritTypes.HEART_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.HeartCrit} ";
        //     }
        //
        //     if (woundedPed.Crits.Has(CritTypes.LUNGS_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.LungsCrit} ";
        //     }
        //
        //     if (woundedPed.Crits.Has(CritTypes.STOMACH_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.StomachCrit} ";
        //     }
        //
        //     if (woundedPed.Crits.Has(CritTypes.GUTS_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.GutsCrit} ";
        //     }
        //
        //     if (woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.ArmsCrit} ";
        //     }
        //
        //     if (woundedPed.Crits.Has(CritTypes.LEGS_DAMAGED))
        //     {
        //         healthInfo += $"{_locale.Data.LegsCrit} ";
        //     }
        //
        //     if (!string.IsNullOrEmpty(healthInfo))
        //     {
        //         SendMessage(healthInfo + "~s~");
        //     }
        // }
    }
}