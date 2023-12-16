namespace GunshotWound2.WoundFeature {
    public sealed class SmallCaliberDamage : BaseGunDamage {
        protected override bool CanPenetrateArmor => true;
        protected override float HelmetSafeChance => 0.8f;

        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 2;
        protected override int PenetratingWoundWeight => 6;
        protected override int PerforatingWoundWeight => 1;
        protected override int AvulsiveWoundWeight => 1;

        public SmallCaliberDamage(SharedData sharedData) : base(sharedData, "SmallCaliber") { }
    }
}