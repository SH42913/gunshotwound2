namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class ArmsCrit : BaseCrit {
        private const float SHAKE_AMPLITUDE = 1f;
        private static readonly int[] NM_MESSAGES = { 155, };

        protected override string CritName => sharedData.localeConfig.ArmsCrit;
        protected override string PlayerMessage => sharedData.localeConfig.PlayerArmsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManArmsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanArmsCritMessage;

        public ArmsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = true;
            convertedPed.nmMessages = NM_MESSAGES;
            convertedPed.thisPed.PlayAmbientSpeech("DEATH_HIGH_MEDIUM", GTA.SpeechModifier.InterruptShouted);
            CreatePain(pedEntity, 20f);

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

        public override void EveryFrame(Entity pedEntity, ref ConvertedPed convertedPed) { }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = false;
            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeCritType = false;
                sharedData.cameraService.aimingShakeAmplitude -= SHAKE_AMPLITUDE;
            }
        }
    }
}