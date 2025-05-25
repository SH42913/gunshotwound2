namespace GunshotWound2.CritsFeature {
    using System;
    using Configs;
    using HitDetection;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Crits : IComponent {
        [Flags]
        public enum Effects {
            Nothing = 0,
            LegsCrit = 1 << 0,
            ArmsCrit = 1 << 1,
            SpineCrit = 1 << 2,
            GutsCrit = 1 << 3,
            StomachCrit = 1 << 4,
            LungsCrit = 1 << 5,
            HeartCrit = 1 << 6,
        }

        public BodyPartConfig.BodyPart requestBodyPart;
        public Effects activeEffects;
    }

    public static class CritsExtensions {
        public static bool HasActive(this in Crits crits, Crits.Effects value) {
            return (crits.activeEffects & value) == value;
        }
    }
}