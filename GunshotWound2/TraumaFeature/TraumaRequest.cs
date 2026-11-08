namespace GunshotWound2.TraumaFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;

    [Serializable]
    public struct TraumaRequest : IRequestData {
        public EcsEntity target;
        public EcsEntity parentBleeding;
        public BodyPartConfig.BodyPart targetBodyPart;
    }
}