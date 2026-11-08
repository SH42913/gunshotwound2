namespace GunshotWound2.PainFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct PainGenerator : IComponent {
        public EcsEntity target;
        public float moveRate;
        public float runRate;
        public float aimRate;
    }
}