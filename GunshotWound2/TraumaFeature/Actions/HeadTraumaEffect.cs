namespace GunshotWound2.TraumaFeature {
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class HeadTraumaEffect : SpineTraumaEffect {
        public override string PlayerMessage => null;
        public override string ManMessage => null;
        public override string WomanMessage => null;

        public HeadTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            base.Apply(entity, ref convertedPed);
            convertedPed.thisPed.IsPainAudioEnabled = false;
            convertedPed.thisPed.StopCurrentPlayingAmbientSpeech();
            convertedPed.thisPed.StopCurrentPlayingSpeech();

            string speech;
            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(true);
                speech = sharedData.random.Next(StatusFeature.Statuses.UnconsciousStatus.PLAYER_DEATH_AMBIENT);
            } else {
                speech = sharedData.random.Next(StatusFeature.Statuses.UnconsciousStatus.NON_PLAYER_DEATH_AMBIENT);
            }

            convertedPed.thisPed.PlayAmbientSpeech(speech);

            string facialAnimName = sharedData.random.Next(StatusFeature.Statuses.UnconsciousStatus.DEATH_MOODS);
            convertedPed.RequestFacialIdleAnim(facialAnimName);
            convertedPed.facialIdleAnimLock = true;

            if (convertedPed.thisPed.CurrentVehicle.IsValid()) {
                GTAHelpers.PlayDeathAnimationInVehicle(convertedPed.thisPed);
            }
        }

        public override void Repeat(Entity entity, ref ConvertedPed convertedPed) {
            base.Repeat(entity, ref convertedPed);

            string reason = sharedData.localeConfig.GetTranslation("Traumas.BleedingInHead");
            entity.GetComponent<Health>().InstantKill(reason);
        }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            base.Cancel(entity, ref convertedPed);
            convertedPed.thisPed.IsPainAudioEnabled = true;
            convertedPed.facialIdleAnimLock = false;
            convertedPed.ResetFacialIdleAnim();

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(false);
            }

            if (convertedPed.thisPed.CurrentVehicle.IsValid()) {
                convertedPed.thisPed.Task.ClearAll();
            }
        }
    }
}