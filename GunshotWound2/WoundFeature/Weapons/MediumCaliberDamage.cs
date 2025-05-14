namespace GunshotWound2.WoundFeature {
    using Configs;

    public sealed class MediumCaliberDamage : BaseGunDamage {
        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 2;
        protected override int PenetratingWoundWeight => 5;
        protected override int PerforatingWoundWeight => 6;
        protected override int AvulsiveWoundWeight => 1;
        protected override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.MediumCaliber;

        public MediumCaliberDamage(SharedData sharedData) : base(sharedData) { }
    }
}