namespace GunshotWound2.WoundFeature {
    public sealed class HeavyCaliberDamage : BaseGunDamage {
        protected override bool CanPenetrateArmor => true;
        protected override float HelmetSafeChance => 0.05f;

        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 1;
        protected override int PenetratingWoundWeight => 2;
        protected override int PerforatingWoundWeight => 2;
        protected override int AvulsiveWoundWeight => 4;

        public HeavyCaliberDamage(SharedData sharedData) : base(sharedData, "HighCaliber") { }
    }
}