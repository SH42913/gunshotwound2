namespace GunshotWound2.WoundFeature {
    using Configs;

    public sealed class SmallCaliberDamage : BaseGunDamage {
        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 2;
        protected override int PenetratingWoundWeight => 6;
        protected override int PerforatingWoundWeight => 1;
        protected override int AvulsiveWoundWeight => 1;
        protected override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.SmallCaliber;

        public SmallCaliberDamage(SharedData sharedData) : base(sharedData) { }
    }
}