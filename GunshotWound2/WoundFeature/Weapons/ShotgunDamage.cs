namespace GunshotWound2.WoundFeature {
    using Configs;

    public sealed class ShotgunDamage : BaseGunDamage {
        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 1;
        protected override int PenetratingWoundWeight => 5;
        protected override int PerforatingWoundWeight => 0;
        protected override int AvulsiveWoundWeight => 2;
        protected override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.Shotgun;

        public ShotgunDamage(SharedData sharedData) : base(sharedData) { }
    }
}