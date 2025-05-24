namespace GunshotWound2.CritsFeature {
    using GTA;
    using HitDetection;
    using PedsFeature;

    public sealed class LungsCrit : BaseCrit {
        protected override string CritName => sharedData.localeConfig.LungsCrit;
        protected override string PlayerMessage => sharedData.localeConfig.PlayerLungsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLungsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLungsCritMessage;

        public LungsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, sharedData.mainConfig.bodyPartConfig.GetBodyPartByKey("Chest"), 1f);

            convertedPed.thisPed.PlayAmbientSpeech("COUGH", SpeechModifier.InterruptShouted);
            convertedPed.BlockSprint();

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetLungsCritEffect(true);
            }
        }

        public override void EveryFrame(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) { }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetLungsCritEffect(false);
            }
        }
    }
}