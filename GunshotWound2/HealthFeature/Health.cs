namespace GunshotWound2.HealthFeature {
    using System;
    using System.Collections.Generic;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Health : IComponent {
        public int max;
        public float diff;
        public bool kill;
        public string lastDamageReason;

        public float bleedingHealRate;
        public Entity bleedingToBandage;
        public HashSet<Entity> bleedingWounds;
        public float timeToBandage;
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
    }
}