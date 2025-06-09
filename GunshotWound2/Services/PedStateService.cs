namespace GunshotWound2.Services {
    using System.Collections.Generic;
    using System.Text;
    using Configs;
    using HealthFeature;
    using InventoryFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class PedStateService {
        private readonly struct BleedDesc {
            public readonly string name;
            public readonly string reason;
            public readonly float severity;
            public readonly string bodyPartLocKey;
            public readonly bool toBeBandaged;

            private readonly int time;
            private readonly bool isTrauma;

            public BleedDesc(in Bleeding bleeding, bool toBeBandaged) {
                name = bleeding.name;
                reason = bleeding.reason;
                severity = bleeding.severity;
                bodyPartLocKey = bleeding.bodyPart.LocKey;
                this.toBeBandaged = toBeBandaged;

                time = bleeding.processedTime;
                isTrauma = bleeding.isTrauma;
            }

            public int CompareTo(in BleedDesc other) {
                int timeCompare = time.CompareTo(other.time);
                return timeCompare == 0
                        ? isTrauma.CompareTo(other.isTrauma)
                        : timeCompare;
            }
        }

        private readonly Notifier notifier;
        private readonly MainConfig mainConfig;
        private readonly LocaleConfig localeConfig;

        private readonly StringBuilder stringBuilder = new();
        private readonly List<BleedDesc> bleedBuffer = new();
        private int lastPost;

        public PedStateService(Notifier notifier, MainConfig mainConfig, LocaleConfig localeConfig) {
            this.notifier = notifier;
            this.mainConfig = mainConfig;
            this.localeConfig = localeConfig;
        }

        public void Check(Entity pedEntity) {
            notifier.HideAllLast();

            string message = BuildString(pedEntity);
            lastPost = notifier.ReplaceOne(message, blinking: true, lastPost);
        }

        public string BuildString(Entity pedEntity) {
            ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
            ref Health health = ref pedEntity.GetComponent<Health>();
            ref Inventory inventory = ref pedEntity.GetComponent<Inventory>();
            ref CurrentlyUsingItem currentlyUsing = ref pedEntity.GetComponent<CurrentlyUsingItem>();

            stringBuilder.Clear();
            stringBuilder.Append("<C></C>");
            ShowHealth(ref convertedPed, ref health);
            ShowPain(pedEntity, health);
            ShowArmor(ref convertedPed);
            ShowBleedingWounds(pedEntity, ref convertedPed, ref health, ref currentlyUsing);
            ShowInventory(ref inventory, ref currentlyUsing, health);
            return stringBuilder.ToString();
        }

        private void ShowHealth(ref ConvertedPed convertedPed, ref Health health) {
            string healthType = convertedPed.isPlayer
                    ? localeConfig.YourHealth
                    : convertedPed.isMale
                            ? localeConfig.HisHealth
                            : localeConfig.HerHealth;

            stringBuilder.SetDefaultColor();
            if (health.isDead) {
                stringBuilder.Append(localeConfig.DeathReason);
                stringBuilder.Append(": ");
                Notifier.Color.RED.AppendTo(stringBuilder);

                string reason = health.lastDamageReason;
                if (!string.IsNullOrEmpty(health.lastDamageReason)) {
                    stringBuilder.Append(reason);
                } else if (!health.mostDangerousBleeding.IsNullOrDisposed()) {
                    string name = health.mostDangerousBleeding.GetComponent<Bleeding>().name;
                    stringBuilder.AppendFormat("{0} ({1})", localeConfig.BleedingReason, name);
                } else {
                    stringBuilder.AppendFormat("UNKNOWN");
                }
            } else {
                int currentHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
                int maxHealth = WoundConfig.ConvertHealthFromNative(health.max);
                float healthPercent = (float)currentHealth / maxHealth;

                stringBuilder.Append(healthType);
                stringBuilder.Append(": ");

                Notifier.Color color = healthPercent switch {
                    >= 0.85f => Notifier.Color.GREEN,
                    > 0.6f   => Notifier.Color.YELLOW,
                    > 0.3f   => Notifier.Color.ORANGE,
                    _        => Notifier.Color.RED,
                };

                ShowGauge(healthPercent, color);

#if DEBUG
                stringBuilder.SetDefaultColor();
                stringBuilder.AppendFormat(" ({0})", healthPercent.ToString("P0"));
#endif
            }

            stringBuilder.SetDefaultColor();
        }

        private void ShowArmor(ref ConvertedPed convertedPed) {
            if (!mainConfig.armorConfig.TryGetArmorLevel(convertedPed.thisPed.Armor, out ArmorConfig.Level armorLevel)) {
                return;
            }

            string message = localeConfig.GetTranslation(armorLevel.LocKey);
            string color = armorLevel.ColorPrefix;
            stringBuilder.AppendEndOfLine();
            stringBuilder.Append(color);
            stringBuilder.Append(message);
#if DEBUG
            stringBuilder.AppendFormat(" ({0})", convertedPed.thisPed.Armor.ToString());
#endif
            stringBuilder.SetDefaultColor();
        }

        private void ShowPain(Entity pedEntity, in Health health) {
            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            if (pain.currentState == null || health.isDead) {
                return;
            }

            stringBuilder.AppendEndOfLine();
            stringBuilder.Append(localeConfig.Pain);
            stringBuilder.Append(": ");

            float painPercent = pain.Percent();
            ShowGauge(painPercent, pain.currentState.Color);

#if DEBUG
            stringBuilder.SetDefaultColor();
            stringBuilder.AppendFormat(" ({0})", painPercent.ToString("P0"));
#endif

            if (pain.TooMuchPain()) {
                Notifier.Color.RED.AppendTo(stringBuilder);
                stringBuilder.AppendFormat(" ({0} sec)", pain.TimeToRecover().ToString("F1"));
            } else if (pain.delayedDiff > 0f) {
                Notifier.Color.ORANGE.AppendTo(stringBuilder);
                stringBuilder.AppendFormat(" (+{0})", pain.DelayedPercent().ToString("P1"));
            }

            ref PainkillersEffect painkillersEffect = ref pedEntity.GetComponent<PainkillersEffect>(out bool painkillersActive);
            if (painkillersActive) {
                float remainingTime = painkillersEffect.remainingTime;
                stringBuilder.AppendEndOfLine();
                Notifier.Color.GREEN.AppendTo(stringBuilder);
                stringBuilder.AppendFormat("{0}: {1} sec", localeConfig.PainkillersRemainingTime, remainingTime.ToString("F1"));
            }

            stringBuilder.SetDefaultColor();
        }

        private void ShowBleedingWounds(Entity pedEntity,
                                        ref ConvertedPed convertedPed,
                                        ref Health health,
                                        ref CurrentlyUsingItem currentlyUsing) {
            if (!health.HasBleedingWounds()) {
                return;
            }

            stringBuilder.AppendEndOfLine();
            stringBuilder.AppendEndOfLine();
            stringBuilder.SetDefaultColor();
            stringBuilder.Append(localeConfig.Wounds);
            stringBuilder.Append(':');

            foreach (Entity bleedingEntity in health.bleedingWounds) {
                ref Bleeding bleeding = ref bleedingEntity.GetComponent<Bleeding>();
                if (bleeding.isProcessed) {
                    bool isBleedingToBandage = health.bleedingToBandage == bleedingEntity;
                    bleedBuffer.Add(new BleedDesc(bleeding, isBleedingToBandage));
                }
            }

            bleedBuffer.Sort((x, y) => x.CompareTo(y));
            foreach (BleedDesc bleeding in bleedBuffer) {
                stringBuilder.AppendEndOfLine();
                if (bleeding.toBeBandaged) {
                    stringBuilder.Append("~g~-> ");
                }

                Notifier.Color color = health.GetBleedingColor(convertedPed, bleeding.severity);
                color.AppendTo(stringBuilder);

                string bodyPart = localeConfig.GetTranslation(bleeding.bodyPartLocKey);
                if (string.IsNullOrEmpty(bleeding.reason)) {
                    stringBuilder.AppendFormat("{0} ({1})", bleeding.name, bodyPart);
                } else {
                    stringBuilder.AppendFormat("{0} ({1}, {2})", bleeding.name, bodyPart, bleeding.reason);
                }

                if (bleeding.toBeBandaged && currentlyUsing.itemTemplate.IsBandage() && currentlyUsing.target == pedEntity) {
                    stringBuilder.AppendFormat(" ~g~({0})", currentlyUsing.ProgressPercent().ToString("P1"));
                }
            }

            stringBuilder.SetDefaultColor();
            bleedBuffer.Clear();
        }

        private void ShowInventory(ref Inventory inventory, ref CurrentlyUsingItem currentlyUsing, in Health health) {
            if (inventory.items == null || inventory.items.Count < 1 || health.isDead) {
                return;
            }

            stringBuilder.AppendLine();
            stringBuilder.Append(localeConfig.YourInventory);

            foreach ((ItemTemplate item, int count) in inventory.items) {
                stringBuilder.AppendEndOfLine();
                stringBuilder.Append(item.GetPluralTranslation(localeConfig, count));

                if (item.Equals(currentlyUsing.itemTemplate)) {
                    stringBuilder.AppendFormat(" ({0})", currentlyUsing.ProgressPercent().ToString("P1"));
                }
            }
        }

        private void ShowGauge(float targetPercent, in Notifier.Color baseColor) {
            stringBuilder.Append("<C>");
            baseColor.AppendTo(stringBuilder);

            var greyColor = false;
            for (int i = 0, max = 10; i < max; i++) {
                float symbolPercent = (float)(i + 1) / max;
                if (!greyColor && symbolPercent > targetPercent) {
                    Notifier.Color.GREY.AppendTo(stringBuilder);
                    greyColor = true;
                }

                stringBuilder.Append('|');
            }

            stringBuilder.Append("</C>");
        }
    }
}