namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using PlayerFeature;

    public sealed class HeartCrit : BaseCrit {
        private const string POST_FX = "DrugsDrivingIn";
        private readonly int[] nmMessages = {
            1083,
        };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerHeartCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManHeartCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanHeartCritMessage;

        public HeartCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, 2.5f);

            convertedPed.RequestRagdoll(6000);
            convertedPed.nmMessages = nmMessages;

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