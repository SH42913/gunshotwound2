namespace GunshotWound2.TraumaFeature {
    using System;
    using Configs;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Traumas : IComponent {
        [Flags]
        public enum Effects {
            Nothing = 0,
            LegsCrit = 1 << 0,
            ArmsCrit = 1 << 1,
            SpineCrit = 1 << 2,
            AbdomenCrit = 1 << 3,
            LungsCrit = 1 << 4,
            HeartCrit = 1 << 5,
        }

        public BodyPartConfig.BodyPart requestBodyPart;
        public Effects activeEffects;
    }

    public static class TraumasExtensions {
        public static bool HasActive(this in Traumas traumas, Traumas.Effects value) {
            return (traumas.activeEffects & value) == value;
        }
    }
}