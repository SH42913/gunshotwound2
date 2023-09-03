namespace GunshotWound2.Damage {
    public sealed class MediumCaliberDamage : BaseGunDamage {
        protected override bool CanPenetrateArmor => true;
        protected override float HelmetSafeChance => 0.3f;

        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 2;
        protected override int PenetratingWoundWeight => 5;
        protected override int PerforatingWoundWeight => 6;
        protected override int AvulsiveWoundWeight => 1;

        public MediumCaliberDamage(SharedData sharedData) : base(sharedData, "MediumCaliber") { }
    }
}