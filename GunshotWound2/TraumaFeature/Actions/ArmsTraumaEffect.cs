namespace GunshotWound2.TraumaFeature {
    using Configs;
    using GTA;
    using GTA.NaturalMotion;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class ArmsTraumaEffect : BaseTraumaEffect {
        private const float SHAKE_AMPLITUDE = 1f;
        private static readonly int[] NM_MESSAGES = { 155, }; // configureShotInjuredArm

        public override string PlayerMessage => sharedData.localeConfig.PlayerArmsCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManArmsCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanArmsCritMessage;

        public ArmsTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(EcsEntity entity, in BodyPartConfig.BodyPart bodyPart, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = true;
            convertedPed.thisPed.PlayAmbientSpeech("DEATH_HIGH_MEDIUM", SpeechModifier.InterruptShouted);

            if (!convertedPed.isPlayer) {
                convertedPed.combatPerformanceMult = 0.1f;
            } else {
                sharedData.cameraService.aimingShakeCritType = true;
                sharedData.cameraService.aimingShakeAmplitude += SHAKE_AMPLITUDE;
            }

            entity.world.GetRequest<WeaponDropRequest>().Publish(new WeaponDropRequest {
                target = entity,
            });

            if (convertedPed.thisPed.IsClimbing || convertedPed.thisPed.IsClimbingLadder) {
                convertedPed.RequestRagdoll(1000);
            }

            if (convertedPed.activeNmHelper is InjuredOnGroundHelper injuredOnGroundHelper) {
                injuredOnGroundHelper.DontReachWithLeft = bodyPart.IsLeftArm();
                injuredOnGroundHelper.DontReachWithRight = bodyPart.IsRightArm();
                injuredOnGroundHelper.Update();
            }
        }

        public override void Repeat(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void Cancel(EcsEntity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = false;
            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeCritType = false;
                sharedData.cameraService.aimingShakeAmplitude -= SHAKE_AMPLITUDE;
            } else {
                convertedPed.combatPerformanceMult = 1f;
            }
        }
    }
}