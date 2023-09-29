namespace GunshotWound2.HealthCare {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Health : IComponent {
        public int max;
        public float damage;
    }
}