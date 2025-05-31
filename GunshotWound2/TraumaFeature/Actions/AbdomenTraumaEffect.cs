namespace GunshotWound2.TraumaFeature {
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class AbdomenTraumaEffect : BaseTraumaEffect {
        private const float NEW_BLEEDING_CHANCE = 0.25f / 100f;
        private const float BLEEDING_SEVERITY = 0.5f;

        private static readonly int[] NM_MESSAGES = { 1119, };

        public override string PlayerMessage => sharedData.localeConfig.PlayerStomachCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManStomachCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanStomachCritMessage;

        public AbdomenTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS", GTA.SpeechModifier.InterruptShouted);

            convertedPed.RequestRagdoll(4000);
            convertedPed.nmMessages = NM_MESSAGES;
        }

        public override void EveryFrame(Entity entity, ref ConvertedPed convertedPed) {
            if (!convertedPed.thisPed.IsRunning && !convertedPed.thisPed.IsSprinting) {
                return;
            }

            bool openNewBleeding = sharedData.random.IsTrueWithProbability(NEW_BLEEDING_CHANCE);
            if (openNewBleeding) {
                BodyPartConfig.BodyPart bodyPart = sharedData.mainConfig.bodyPartConfig.GetBodyPartByKey("Abdomen");
                CreateInternalBleeding(entity, bodyPart, 0.5f * BLEEDING_SEVERITY);
                ShowRunningWarningMessage(convertedPed);
            }
        }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) { }
    }
}