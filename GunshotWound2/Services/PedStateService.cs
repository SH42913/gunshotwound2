namespace GunshotWound2.Services {
    using System.Text;
    using Configs;
    using CritsFeature;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class PedStateService {
        private readonly Notifier notifier;
        private readonly MainConfig mainConfig;
        private readonly LocaleConfig localeConfig;

        private readonly StringBuilder stringBuilder = new();
        private int lastPost;

        public PedStateService(Notifier notifier, MainConfig mainConfig, LocaleConfig localeConfig) {
            this.notifier = notifier;
            this.mainConfig = mainConfig;
            this.localeConfig = localeConfig;
        }

        public void Check(Entity pedEntity) {
            string message = BuildString(pedEntity);
            lastPost = notifier.ReplaceOne(message, blinking: true, lastPost);
        }

        public string BuildString(Entity pedEntity) {
            ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
            ref Health health = ref pedEntity.GetComponent<Health>();

            stringBuilder.Clear();
            ShowHealth(ref convertedPed, ref health);
            ShowPain(pedEntity);
            ShowCrits(pedEntity);
            ShowArmor(ref convertedPed);
            ShowBleedingWounds(ref convertedPed, ref health);
            return stringBuilder.ToString();
        }

        private void ShowHealth(ref ConvertedPed convertedPed, ref Health health) {
            string healthType = convertedPed.isPlayer
                    ? localeConfig.YourHealth
                    : convertedPed.isMale
                            ? localeConfig.HisHealth
                            : localeConfig.HerHealth;

            int currentHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
            int maxHealth = WoundConfig.ConvertHealthFromNative(health.max);
            float healthPercent = (float)currentHealth / maxHealth;
            string healthPercentText = healthPercent <= 0 ? localeConfig.Dead : healthPercent.ToString("P0");

            Notifier.Color color;
            if (healthPercent >= 0.75f) {
                color = Notifier.Color.GREEN;
            } else if (healthPercent > 0.5f) {
                color = Notifier.Color.YELLOW;
            } else if (healthPercent > 0.25f) {
                color = Notifier.Color.ORANGE;
            } else {
                color = Notifier.Color.RED;
            }

            stringBuilder.SetDefaultColor();
            stringBuilder.Append(healthType);
            stringBuilder.Append(": ");
            stringBuilder.Append(color);
            stringBuilder.Append(healthPercentText);
            if (healthPercent <= 0f) {
                stringBuilder.AppendEndOfLine();
                stringBuilder.SetDefaultColor();
                stringBuilder.Append(localeConfig.DeathReason);
                stringBuilder.Append(": ");
                stringBuilder.Append(color);
                stringBuilder.Append(health.lastDamageReason ?? "UNKNOWN");
                stringBuilder.AppendEndOfLine();
            }

            stringBuilder.SetDefaultColor();
        }

        private void ShowArmor(ref ConvertedPed convertedPed) {
            int armor = convertedPed.thisPed.Armor;
            if (armor <= 0) {
                return;
            }

            Notifier.Color color;
            string message;
            switch (armor) {
                case > 80:
                    color = Notifier.Color.GREEN;
                    message = localeConfig.ArmorClassV;
                    break;
                case > 60:
                    color = Notifier.Color.GREEN;
                    message = localeConfig.ArmorClassIV;
                    break;
                case > 40:
                    color = Notifier.Color.YELLOW;
                    message = localeConfig.ArmorClassIII;
                    break;
                case > 20:
                    color = Notifier.Color.ORANGE;
                    message = localeConfig.ArmorClassII;
                    break;
                default:
                    color = Notifier.Color.RED;
                    message = localeConfig.ArmorClassI;
                    break;
            }

            stringBuilder.AppendEndOfLine();
            stringBuilder.Append(color);
            stringBuilder.Append(message);
#if DEBUG
            stringBuilder.Append($" ({armor.ToString()})");
#endif
            stringBuilder.SetDefaultColor();
        }

        private void ShowPain(Entity pedEntity) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.currentState == null) {
                return;
            }

            float painPercent = pain.Percent();
            stringBuilder.AppendEndOfLine();
            stringBuilder.Append(localeConfig.Pain);
            stringBuilder.Append(": ");
            stringBuilder.Append(pain.currentState.Color);
            stringBuilder.Append(painPercent.ToString("P1"));

            if (pain.TooMuchPain()) {
                stringBuilder.Append($" ({pain.TimeToRecover().ToString("F1")} sec)");
            }

            stringBuilder.SetDefaultColor();
        }

        private void ShowBleedingWounds(ref ConvertedPed convertedPed, ref Health health) {
            if (health.bleedingWounds == null || health.bleedingWounds.Count < 1) {
                return;
            }

            stringBuilder.AppendEndOfLine();
            stringBuilder.AppendEndOfLine();
            stringBuilder.SetDefaultColor();
            stringBuilder.Append(localeConfig.Wounds);
            stringBuilder.Append(':');

            foreach (Entity bleedingEntity in health.bleedingWounds) {
                ref Bleeding bleeding = ref bleedingEntity.GetComponent<Bleeding>();
                bool isBleedingToBandage = health.bleedingToBandage == bleedingEntity;

                stringBuilder.AppendEndOfLine();
                if (isBleedingToBandage) {
                    stringBuilder.Append("~g~-> ");
                }

                Notifier.Color color = health.GetBleedingColor(convertedPed, bleeding.severity);
                stringBuilder.Append(color);
                stringBuilder.Append(bleeding.name);

                if (isBleedingToBandage && health.timeToBandage > 0f) {
                    float bandagingProgress = 1f - (health.timeToBandage / mainConfig.WoundConfig.ApplyBandageTime);
                    stringBuilder.Append($" ~g~({bandagingProgress.ToString("P1")})");
                }
            }

            stringBuilder.SetDefaultColor();
        }

        private void ShowCrits(Entity pedEntity) {
            ref Crits crits = ref pedEntity.GetComponent<Crits>();
            if (crits.active == Crits.Types.Nothing) {
                return;
            }

            stringBuilder.AppendEndOfLine();
            stringBuilder.Append(localeConfig.Crits);
            stringBuilder.Append(" ~r~");

            if (crits.HasActive(Crits.Types.SpineDamaged)) {
                stringBuilder.Append(localeConfig.NervesCrit);
                stringBuilder.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.HeartDamaged)) {
                stringBuilder.Append(localeConfig.HeartCrit);
                stringBuilder.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.LungsDamaged)) {
                stringBuilder.Append(localeConfig.LungsCrit);
                stringBuilder.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.StomachDamaged)) {
                stringBuilder.Append(localeConfig.StomachCrit);
                stringBuilder.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.GutsDamaged)) {
                stringBuilder.Append(localeConfig.GutsCrit);
                stringBuilder.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.ArmsDamaged)) {
                stringBuilder.Append(localeConfig.ArmsCrit);
                stringBuilder.AppendSpace();
            }

            if (crits.HasActive(Crits.Types.LegsDamaged)) {
                stringBuilder.Append(localeConfig.LegsCrit);
                stringBuilder.AppendSpace();
            }

            stringBuilder.SetDefaultColor();
        }
    }
}