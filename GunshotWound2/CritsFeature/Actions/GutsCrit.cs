namespace GunshotWound2.CritsFeature {
    using HitDetection;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class GutsCrit : BaseCrit {
        private const float NEW_BLEEDING_CHANCE = 0.25f / 100f;
        private const float RUN_PAIN_MULT = 1f;
        private const float BLEEDING_SEVERITY = 0.5f;

        private static readonly int[] NM_MESSAGES = { 1119, };

        protected override string CritName => sharedData.localeConfig.GutsCrit;
        protected override string PlayerMessage => sharedData.localeConfig.PlayerGutsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManGutsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanGutsCritMessage;

        public GutsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS", GTA.SpeechModifier.InterruptShouted);
            convertedPed.RequestRagdoll(4000);
            convertedPed.nmMessages = NM_MESSAGES;
        }

        public override void EveryFrame(Entity pedEntity, ref ConvertedPed convertedPed) {
            if (!convertedPed.thisPed.IsRunning && !convertedPed.thisPed.IsSprinting) {
                return;
            }

            CreatePain(pedEntity, RUN_PAIN_MULT * sharedData.deltaTime);
            bool openNewBleeding = sharedData.random.IsTrueWithProbability(NEW_BLEEDING_CHANCE);
            if (openNewBleeding) {
                CreateInternalBleeding(pedEntity, sharedData.mainConfig.bodyPartConfig.GetBodyPartByKey("Abdomen"), 0.5f * BLEEDING_SEVERITY);
                ShowRunningWarningMessage(convertedPed);
            }
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) { }
    }
}