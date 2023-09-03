namespace GunshotWound2.Damage {
    public sealed class ShotgunDamage : BaseGunDamage {
        protected override bool CanPenetrateArmor => false;
        protected override float HelmetSafeChance => 0.5f;

        protected override int GrazeWoundWeight => 1;
        protected override int FleshWoundWeight => 1;
        protected override int PenetratingWoundWeight => 5;
        protected override int PerforatingWoundWeight => 0;
        protected override int AvulsiveWoundWeight => 2;

        public ShotgunDamage(SharedData sharedData) : base(sharedData, "Shotgun") { }
    }
}