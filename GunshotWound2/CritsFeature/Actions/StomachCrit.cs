namespace GunshotWound2.CritsFeature {
    using HitDetection;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class StomachCrit : BaseCrit {
        private const float NEW_BLEEDING_CHANCE = 0.25f / 100f;
        private const float RUN_PAIN_MULT = 1f;
        private const float BLEEDING_SEVERITY = 0.5f;

        protected override string CritName => sharedData.localeConfig.StomachCrit;
        protected override string PlayerMessage => sharedData.localeConfig.PlayerStomachCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManStomachCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanStomachCritMessage;

        public StomachCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, PedHitData.BodyParts.Abdomen, BLEEDING_SEVERITY);
            convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS", GTA.SpeechModifier.InterruptShouted);
        }

        public override void EveryFrame(Entity pedEntity, ref ConvertedPed convertedPed) {
            if (!convertedPed.thisPed.IsRunning && !convertedPed.thisPed.IsSprinting) {
                return;
            }

            CreatePain(pedEntity, RUN_PAIN_MULT * sharedData.deltaTime);
            bool openNewBleeding = sharedData.random.IsTrueWithProbability(NEW_BLEEDING_CHANCE);
            if (openNewBleeding) {
                CreateInternalBleeding(pedEntity, PedHitData.BodyParts.Abdomen, 0.5f * BLEEDING_SEVERITY);
                ShowRunningWarningMessage(convertedPed);
            }
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) { }
    }
}