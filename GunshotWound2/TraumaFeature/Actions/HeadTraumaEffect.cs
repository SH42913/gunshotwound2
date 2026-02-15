namespace GunshotWound2.TraumaFeature {
    using Configs;
    using GTA;
    using GTA.NaturalMotion;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public sealed class HeadTraumaEffect : SpineTraumaEffect {
        private static readonly Message STIFFNESS_MESSAGE = new("setStiffness");

        public override string PlayerMessage => null;
        public override string ManMessage => null;
        public override string WomanMessage => null;

        public HeadTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(EcsEntity entity, in BodyPartConfig.BodyPart bodyPart, ref ConvertedPed convertedPed) {
            base.Apply(entity, bodyPart, ref convertedPed);

            Ped ped = convertedPed.thisPed;
            ped.IsPainAudioEnabled = false;
            ped.StopCurrentPlayingAmbientSpeech();
            ped.StopCurrentPlayingSpeech();

            string speech;
            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(true);
                speech = sharedData.random.Next(StatusFeature.Statuses.UnconsciousStatus.PLAYER_DEATH_AMBIENT);
            } else {
                speech = sharedData.random.Next(StatusFeature.Statuses.UnconsciousStatus.NON_PLAYER_DEATH_AMBIENT);
            }

            ped.PlayAmbientSpeech(speech);

            string facialAnimName = sharedData.random.Next(StatusFeature.Statuses.UnconsciousStatus.DEATH_MOODS);
            convertedPed.RequestFacialIdleAnim(facialAnimName);
            convertedPed.facialIdleAnimLock = true;
        }

        public override void Repeat(EcsEntity entity, ref ConvertedPed convertedPed) {
            base.Repeat(entity, ref convertedPed);

            string reason = sharedData.localeConfig.GetTranslation("Traumas.BleedingInHead");
            entity.GetComponent<Health>().InstantKill(reason);
        }

        public override void Cancel(EcsEntity entity, ref ConvertedPed convertedPed) {
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

        protected override ConvertedPed.NaturalMotionBuilder GetNMBuilder() {
            return static (sharedData, _, ped) => {
                float stiffness = sharedData.random.NextFloat(4f, 12f);
                STIFFNESS_MESSAGE.SetArgument("bodyStiffness", stiffness);
                STIFFNESS_MESSAGE.SendTo(ped);

                return new BodyRelaxHelper(ped) {
                    Relaxation = sharedData.random.NextFloat(0f, 50f),
                    Damping = sharedData.random.NextFloat(1f, 2f),
                    HoldPose = true,
                };
            };
        }
    }
}