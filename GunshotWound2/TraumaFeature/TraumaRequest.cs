namespace GunshotWound2.TraumaFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;

    [Serializable]
    public struct TraumaRequest : IComponent {
        public EcsEntity target;
        public EcsEntity parentBleeding;
        public BodyPartConfig.BodyPart targetBodyPart;
    }
}