namespace GunshotWound2.TraumaFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct TraumaCauseBleeding : IComponent {
        public Entity traumaEntity;
    }
}