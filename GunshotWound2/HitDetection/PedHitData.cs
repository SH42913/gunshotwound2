namespace GunshotWound2.HitDetection {
    using Scellecs.Morpeh;

    public struct PedHitData : IComponent {
        public enum BodyParts {
            Nothing,
            Head,
            Neck,
            Chest,
            Abdomen,
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
            Stun,
        }

        public BodyParts bodyPart;
        public uint weaponHash;
        public WeaponTypes weaponType;
        public int healthDiff;
        public int armorDiff;
        public bool useRandomBodyPart;
        public bool afterTakedown;
    }
}