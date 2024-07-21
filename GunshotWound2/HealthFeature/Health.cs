namespace GunshotWound2.HealthFeature {
    using System;
    using System.Collections.Generic;
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;

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
    }
}