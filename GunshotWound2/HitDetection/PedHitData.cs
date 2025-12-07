namespace GunshotWound2.HitDetection {
    using Configs;
    using GTA;
    using GTA.Math;
    using Scellecs.Morpeh;

    public struct PedHitData : IComponent {
        public PedBone damagedBone;
        public BodyPartConfig.BodyPart bodyPart;
        public uint weaponHash;
        public WeaponConfig.Weapon weaponType;
        public int healthDiff;
        public int armorDiff;
        public bool useRandomBodyPart;
        public bool afterTakedown;
        public int hits;

        public Ped aggressor;
        public Vector3 shotDir;
        public Vector3 hitPos;
        public Vector3 hitNorm;
    }
}