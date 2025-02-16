namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class GutsCrit : BaseCrit {
        private static readonly int[] NM_MESSAGES = { 1119, };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerGutsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManGutsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanGutsCritMessage;

        public GutsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, 0.5f);

            convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS", GTA.SpeechModifier.InterruptShouted);
            convertedPed.RequestRagdoll(4000);
            convertedPed.nmMessages = NM_MESSAGES;
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) { }
    }
}