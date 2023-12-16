namespace GunshotWound2.HealthFeature {
    using System;
    using System.Collections.Generic;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Health : IComponent {
        public int max;
        public float diff;

        public float bleedingHealRate;
        public Entity bleedingToBandage;
        public HashSet<Entity> bleedingWounds;
        public float timeToBandage;
    }
}