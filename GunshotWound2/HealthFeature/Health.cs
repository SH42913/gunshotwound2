namespace GunshotWound2.HealthFeature {
    using System;
    using System.Collections.Generic;
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    [Serializable]
    public struct Health : IComponent {
        public int max;
        public float diff;
        public bool kill;
        public string lastDamageReason;
        public bool isDead;

        public float bleedingHealRate;
        public Entity bleedingToBandage;
        public HashSet<Entity> bleedingWounds;
        public float timeToBandage;
        public Entity bandagingMedic;
    }

    public static class HealthExtensions {
        public static void DealDamage(this ref Health health, float damage, string reason) {
            health.diff -= damage;
            health.lastDamageReason = reason;
        }

        public static void InstantKill(this ref Health health, string reason) {
            health.kill = true;
            health.lastDamageReason = reason;
        }

        public static bool IsAlive(this ref Health health) {
            return !health.isDead;
        }

        public static float CalculateDeadlyBleedingThreshold(this ref Health health, in ConvertedPed convertedPed) {
            int totalHealth = WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
            float deadlyRate = totalHealth * health.bleedingHealRate;
            return (float)Math.Sqrt(deadlyRate);
        }

        public static Notifier.Color GetBleedingColor(this ref Health health, in ConvertedPed convertedPed, float severity) {
            float threshold = health.CalculateDeadlyBleedingThreshold(convertedPed);
            if (severity > threshold) {
                return Notifier.Color.RED;
            } else if (severity > 0.5f * threshold) {
                return Notifier.Color.ORANGE;
            } else if (severity > 0.25f * threshold) {
                return Notifier.Color.YELLOW;
            } else {
                return Notifier.Color.COMMON;
            }
        }
    }
}