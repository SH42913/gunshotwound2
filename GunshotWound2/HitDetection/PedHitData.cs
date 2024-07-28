namespace GunshotWound2.HitDetection {
    using GTA;
    using Scellecs.Morpeh;

    public struct PedHitData : IComponent {
        public enum BodyParts {
            Nothing,
            Head,
            Neck,
            UpperBody,
            LowerBody,
            Arm,
            Leg,
        }

        public enum WeaponTypes {
            Nothing,
            LightImpact,
            HeavyImpact,
            Cutting,
            SmallCaliber,
            MediumCaliber,
            HeavyCaliber,
            Shotgun,
        }

        public BodyParts bodyPart;
        public WeaponTypes weaponType;
        public int healthDiff;
        public int armorDiff;
        public bool useRandomBodyPart;
        public string armorMessage;
        public Bone damagedBone;
    }
}