namespace GunshotWound2.HitDetection {
    using Configs;
    using Scellecs.Morpeh;

    public struct PedHitData : IComponent {
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

        public BodyPartConfig.BodyPart bodyPart;
        public uint weaponHash;
        public WeaponTypes weaponType;
        public int healthDiff;
        public int armorDiff;
        public bool useRandomBodyPart;
        public bool afterTakedown;
    }
}