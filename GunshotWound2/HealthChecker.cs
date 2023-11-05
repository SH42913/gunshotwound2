namespace GunshotWound2 {
    using System;
    using System.Text;
    using Configs;
    using HealthCare;
    using Peds;
    using Scellecs.Morpeh;
    using Utils;

    public static class HealthChecker {
        private static readonly StringBuilder STRING_BUILDER = new();

        public static void Check(SharedData sharedData, Entity pedEntity) {
            ShowHealth(sharedData, pedEntity);

            // ShowArmor(woundedPed);
            // ShowPain(woundedPed, pedEntity);
            // ShowCrits(woundedPed);

            ShowBleedingWounds(sharedData, pedEntity);
        }

        private static void ShowHealth(SharedData sharedData, Entity pedEntity) {
            LocaleConfig localeConfig = sharedData.localeConfig;

            ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
            ref Health health = ref pedEntity.GetComponent<Health>();

            int currentHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
            int maxHealth = WoundConfig.ConvertHealthFromNative(health.max);
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent <= 0f) {
                sharedData.notifier.info.AddMessage($"~r~{localeConfig.YouAreDead}~s~");
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
            STRING_BUILDER.Append(localeConfig.Health);
            STRING_BUILDER.Append(": ");
            STRING_BUILDER.Append(color);
            STRING_BUILDER.Append(healthPercent.ToString("P0"));
            STRING_BUILDER.SetDefaultColor();

            sharedData.notifier.info.AddMessage(STRING_BUILDER.ToString());
        }

        private static void ShowBleedingWounds(SharedData sharedData, Entity pedEntity) {
            ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
            ref Health health = ref pedEntity.GetComponent<Health>();
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

        // private void ShowPain(WoundedPedComponent woundedPed, int pedEntity)
        // {
        //     var pain = _ecsWorld.GetComponent<PainComponent>(pedEntity);
        //     if (pain == null || woundedPed.IsDead) return;
        //
        //     var painPercent = pain.CurrentPain / woundedPed.MaximalPain;
        //     if (painPercent > 3f)
        //     {
        //         SendMessage($"~s~{_locale.Data.Pain}: ~r~>300%~s~");
        //     }
        //     else
        //     {
        //         var painString = (painPercent * 100f).ToString("0.0");
        //         if (painPercent > 1f)
        //         {
        //             SendMessage($"~s~{_locale.Data.Pain}: ~r~{painString}%~s~");
        //         }
        //         else if (painPercent > 0.5f)
        //         {
        //             SendMessage($"~s~{_locale.Data.Pain}: ~o~{painString}%~s~");
        //         }
        //         else if (painPercent > 0.2f)
        //         {
        //             SendMessage($"~s~{_locale.Data.Pain}: ~y~{painString}%~s~");
        //         }
        //         else if (painPercent > 0f)
        //         {
        //             SendMessage($"~s~{_locale.Data.Pain}: ~g~{painString}%~s~");
        //         }
        //     }
        // }
        //
        // private void ShowArmor(WoundedPedComponent woundedPed)
        // {
        //     if (woundedPed.Armor <= 0) return;
        //     var armorPercent = woundedPed.Armor / 100f;
        //
        //     if (armorPercent > 0.7f)
        //     {
        //         SendMessage($"~g~{_locale.Data.ArmorLooksGreat} ~s~");
        //     }
        //     else if (armorPercent > 0.4f)
        //     {
        //         SendMessage($"~y~{_locale.Data.ScratchesOnArmor} ~s~");
        //     }
        //     else if (armorPercent > 0.15f)
        //     {
        //         SendMessage($"~o~{_locale.Data.DentsOnArmor} ~s~");
        //     }
        //     else
        //     {
        //         SendMessage($"~r~{_locale.Data.ArmorLooksAwful} ~s~");
        //     }
        // }
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