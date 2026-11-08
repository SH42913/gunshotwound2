namespace GunshotWound2.TraumaFeature {
    using Configs;
    using GTA;
    using PedsFeature;

    public sealed class LungsTraumaEffect : BaseTraumaEffect {
        public override string PlayerMessage => sharedData.localeConfig.PlayerLungsCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManLungsCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanLungsCritMessage;

        public LungsTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(EcsEntity entity, in BodyPartConfig.BodyPart bodyPart, ref ConvertedPed convertedPed) {
            convertedPed.thisPed.PlayAmbientSpeech("COUGH", SpeechModifier.InterruptShouted);
            convertedPed.BlockSprint();

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetLungsInjuryEffect(true);
            }
        }

        public override void Repeat(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void Cancel(EcsEntity entity, ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetLungsInjuryEffect(false);
            }
        }
    }
}