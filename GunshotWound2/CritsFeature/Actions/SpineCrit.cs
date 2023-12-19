namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class SpineCrit : BaseCrit {
        protected override string PlayerMessage => sharedData.localeConfig.PlayerNervesCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManNervesCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanNervesCritMessage;

        public SpineCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.RequestPermanentRagdoll();
            convertedPed.hasSpineDamage = true;
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.hasSpineDamage = false;
        }
    }
}