namespace GunshotWound2.HitDetection {
    using GTA;
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

        public Bone damagedBone;
        public BodyParts bodyPart;
        public uint weaponHash;
        public WeaponTypes weaponType;
        public int healthDiff;
        public int armorDiff;
        public bool useRandomBodyPart;
        public string armorMessage;
        public bool afterTakedown;
    }
}