namespace GunshotWound2.TraumaFeature {
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class HeadTraumaEffect : SpineTraumaEffect {
        public override string PlayerMessage => null;
        public override string ManMessage => null;
        public override string WomanMessage => null;

        public HeadTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            base.Apply(entity, ref convertedPed);

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(true);
            }
        }

        public override void Repeat(Entity entity, ref ConvertedPed convertedPed) {
            base.Repeat(entity, ref convertedPed);

            string reason = sharedData.localeConfig.GetTranslation("Traumas.BleedingInHead");
            entity.GetComponent<Health>().InstantKill(reason);
        }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            base.Cancel(entity, ref convertedPed);

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(false);
            }
        }
    }
}