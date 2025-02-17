namespace GunshotWound2.CritsFeature {
    using PedsFeature;

    public sealed class HeartCrit : BaseCrit {
        private static readonly int[] NM_MESSAGES = { 1083, };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerHeartCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManHeartCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanHeartCritMessage;

        public HeartCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, 2.5f);

            convertedPed.thisPed.PlayAmbientSpeech("COUGH", GTA.SpeechModifier.InterruptShouted);
            convertedPed.RequestRagdoll(6000);
            convertedPed.nmMessages = NM_MESSAGES;

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeartCritEffect(true);
            }
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeartCritEffect(false);
            }
        }
    }
}