namespace GunshotWound2.TraumaFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class ArmsTraumaEffect : BaseTraumaEffect {
        private const float SHAKE_AMPLITUDE = 1f;
        private static readonly int[] NM_MESSAGES = { 155, }; // configureShotInjuredArm

        public override string PlayerMessage => sharedData.localeConfig.PlayerArmsCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManArmsCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanArmsCritMessage;

        public ArmsTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = true;
            convertedPed.thisPed.PlayAmbientSpeech("DEATH_HIGH_MEDIUM", GTA.SpeechModifier.InterruptShouted);

            if (!convertedPed.isPlayer) {
                convertedPed.thisPed.Accuracy = (int)(0.1f * convertedPed.defaultAccuracy);
            } else {
                sharedData.cameraService.aimingShakeCritType = true;
                sharedData.cameraService.aimingShakeAmplitude += SHAKE_AMPLITUDE;
            }

            if (!convertedPed.isPlayer || sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            if (convertedPed.thisPed.IsClimbing || convertedPed.thisPed.IsClimbingLadder) {
                convertedPed.RequestRagdoll(1000);
            }
        }

        public override void Repeat(Entity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(Entity entity, ref ConvertedPed convertedPed) { }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = false;
            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeCritType = false;
                sharedData.cameraService.aimingShakeAmplitude -= SHAKE_AMPLITUDE;
            }
        }
    }
}