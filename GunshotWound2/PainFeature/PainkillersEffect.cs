namespace GunshotWound2.PainFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct PainkillersEffect : IComponent {
        public float rate;
        public float remainingTime;
    }
}