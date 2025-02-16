namespace GunshotWound2.CritsFeature {
    using GTA;
    using PedsFeature;

    public sealed class LegsCrit : BaseCrit {
        protected override string PlayerMessage => sharedData.localeConfig.PlayerLegsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLegsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLegsCritMessage;

        public LegsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 20f);

            RagdollType ragdollType = convertedPed.thisPed.IsRunning ? RagdollType.Balance : RagdollType.Relax;
            convertedPed.RequestRagdoll(3000, ragdollType);
            convertedPed.hasBrokenLegs = true;

            convertedPed.moveRate = sharedData.mainConfig.WoundConfig.MoveRateOnLegsCrit;
            convertedPed.BlockSprint();
            convertedPed.thisPed.SetConfigFlag(PedConfigFlagToggles.IsInjured, true);

            convertedPed.thisPed.PlayAmbientSpeech("DEATH_HIGH_MEDIUM", SpeechModifier.InterruptShouted);
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasBrokenLegs = false;
            convertedPed.ResetMoveRate();
            convertedPed.UnBlockSprint();
            convertedPed.thisPed.SetConfigFlag(PedConfigFlagToggles.IsInjured, false);
        }
    }
}