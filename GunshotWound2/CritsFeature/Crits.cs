namespace GunshotWound2.CritsFeature {
    using System;
    using HitDetection;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Crits : IComponent {
        [Flags]
        public enum Types {
            Nothing = 0,
            LegsDamaged = 1 << 0,
            ArmsDamaged = 1 << 1,
            NervesDamaged = 1 << 2,
            GutsDamaged = 1 << 3,
            StomachDamaged = 1 << 4,
            LungsDamaged = 1 << 5,
            HeartDamaged = 1 << 6,
        }

        public PedHitData.BodyParts requestBodyPart;
        public Types active;
    }

    public static class CritsExtensions {
        public static bool HasActive(this in Crits crits, Crits.Types value) {
            return (crits.active & value) == value;
        }
    }
}