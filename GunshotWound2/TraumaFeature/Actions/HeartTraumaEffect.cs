namespace GunshotWound2.TraumaFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class HeartTraumaEffect : BaseTraumaEffect {
        private static readonly int[] NM_MESSAGES = { 1083, };

        public override string PlayerMessage => sharedData.localeConfig.PlayerHeartCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManHeartCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanHeartCritMessage;

        public HeartTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.thisPed.PlayAmbientSpeech("COUGH", GTA.SpeechModifier.InterruptShouted);
            convertedPed.RequestRagdoll(6000);
            convertedPed.nmMessages = NM_MESSAGES;

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeartCritEffect(true);
            }
        }

        public override void EveryFrame(Entity entity, ref ConvertedPed convertedPed) { }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeartCritEffect(false);
            }
        }
    }
}