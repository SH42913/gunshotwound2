namespace GunshotWound2.TraumaFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;

    [Serializable]
    public struct TraumaRequest : IComponent {
        public Entity target;
        public Entity parentBleeding;
        public BodyPartConfig.BodyPart targetBodyPart;
    }
}