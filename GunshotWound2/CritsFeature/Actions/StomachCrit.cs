namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class StomachCrit : BaseCrit {
        protected override string PlayerMessage => sharedData.localeConfig.PlayerStomachCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManStomachCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanStomachCritMessage;

        public StomachCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) { }
    }
}