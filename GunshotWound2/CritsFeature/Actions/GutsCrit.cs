namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class GutsCrit : BaseCrit {
        private readonly int[] nmMessages = {
            1119,
        };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerGutsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManGutsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanGutsCritMessage;

        public GutsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);

            convertedPed.RequestRagdoll(4000);
            convertedPed.nmMessages = nmMessages;
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) { }
    }
}