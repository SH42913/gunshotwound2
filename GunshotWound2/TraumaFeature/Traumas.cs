namespace GunshotWound2.TraumaFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Traumas : IComponent {
        [Flags]
        public enum Effects {
            Nothing = 0,
            Legs = 1 << 0,
            Arms = 1 << 1,
            Spine = 1 << 2,
            Abdomen = 1 << 3,
            Lungs = 1 << 4,
            Heart = 1 << 5,
            Head = 1 << 6,
        }

        public BodyPartConfig.BodyPart requestBodyPart;
        public bool forBluntDamage;
        public Effects activeEffects;
    }

    public static class TraumasExtensions {
        public static bool HasActive(this in Traumas traumas, Traumas.Effects value) {
            return (traumas.activeEffects & value) == value;
        }
    }
}