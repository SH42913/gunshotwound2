namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using PlayerFeature;
    using Utils;

    public sealed class HeartCrit : BaseCrit {
        private const string POST_FX = "DrugsDrivingIn";
        private static readonly int[] PAIN_SOUNDS = { 20, 22, };
        private static readonly int[] NM_MESSAGES = { 1083, };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerHeartCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManHeartCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanHeartCritMessage;

        public HeartCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, 2.5f);

            PedEffects.PlayPain(convertedPed.thisPed, sharedData.random.Next(PAIN_SOUNDS));
            convertedPed.RequestRagdoll(6000);
            convertedPed.nmMessages = NM_MESSAGES;

            if (convertedPed.isPlayer) {
                CameraEffects.StartPostFx(POST_FX, 5000);
            }
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                CameraEffects.StopPostFx(POST_FX);
            }
        }
    }
}