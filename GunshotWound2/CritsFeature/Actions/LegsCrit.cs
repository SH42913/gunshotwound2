namespace GunshotWound2.CritsFeature {
    using GTA;
    using PedsFeature;

    public sealed class LegsCrit : BaseCrit {
        private static readonly int[] NM_MESSAGES = { 169, };
        private const int LEG_INJURE_FLAG = 166;

        protected override string PlayerMessage => sharedData.localeConfig.PlayerLegsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLegsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLegsCritMessage;

        public LegsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 20f);

            RagdollType ragdollType = convertedPed.thisPed.IsRunning ? RagdollType.Balance : RagdollType.Relax;
            convertedPed.RequestRagdoll(3000, ragdollType);
            convertedPed.hasBrokenLegs = true;

            convertedPed.moveRate = sharedData.mainConfig.WoundConfig.MoveRateOnNervesDamage;
            convertedPed.BlockSprint();
            convertedPed.thisPed.SetConfigFlag(LEG_INJURE_FLAG, true);
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasBrokenLegs = false;
            convertedPed.UnBlockSprint();
            convertedPed.thisPed.ResetConfigFlag(LEG_INJURE_FLAG);
        }
    }
}