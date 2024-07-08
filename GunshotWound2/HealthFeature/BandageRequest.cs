namespace GunshotWound2.HealthFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct BandageRequest : IComponent {
        public Entity medic;
    }
}