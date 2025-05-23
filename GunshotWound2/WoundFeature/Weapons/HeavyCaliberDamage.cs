namespace GunshotWound2.WoundFeature {
    using Configs;

    public sealed class HeavyCaliberDamage : BaseGunDamage {
        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 1;
        protected override int PenetratingWoundWeight => 2;
        protected override int PerforatingWoundWeight => 2;
        protected override int AvulsiveWoundWeight => 4;
        public override WeaponConfig.Stats Stats => sharedData.mainConfig.weaponConfig.HeavyCaliber;

        public HeavyCaliberDamage(SharedData sharedData) : base(sharedData) { }
    }
}