namespace GunshotWound2.HitDetection {
    using Configs;
    using Scellecs.Morpeh;

    public struct PedHitData : IComponent {
        public BodyPartConfig.BodyPart bodyPart;
        public uint weaponHash;
        public WeaponConfig.Weapon weaponType;
        public int healthDiff;
        public int armorDiff;
        public bool useRandomBodyPart;
        public bool afterTakedown;
    }
}