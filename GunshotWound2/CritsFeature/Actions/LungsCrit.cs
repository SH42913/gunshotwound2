namespace GunshotWound2.CritsFeature {
    using GTA;
    using PedsFeature;
    using PlayerFeature;

    public sealed class LungsCrit : BaseCrit {
        private const string POST_FX = "DeathFailMPIn";

        protected override string PlayerMessage => sharedData.localeConfig.PlayerLungsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLungsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLungsCritMessage;

        public LungsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, 1f);

            convertedPed.thisPed.PlayAmbientSpeech("COUGH", SpeechModifier.InterruptShouted);
            convertedPed.BlockSprint();

            if (convertedPed.isPlayer) {
                CameraEffects.StartPostFx(POST_FX, 5000);
            }
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                CameraEffects.StopPostFx(POST_FX);
            }
        }
    }
}