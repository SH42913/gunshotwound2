namespace GunshotWound2.PainFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct PainGenerator : IComponent {
        public Entity target;
        public float moveRate;
        public float runRate;
    }
}