namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class ArmsCrit : BaseCrit {
        private static readonly int[] NM_MESSAGES = { 155, };

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
                CameraEffects.ShakeCameraPermanent();
            }

            if (!convertedPed.isPlayer || sharedData.mainConfig.PlayerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            if (convertedPed.thisPed.IsClimbing || convertedPed.thisPed.IsClimbingLadder) {
                convertedPed.RequestRagdoll(1000);
            }
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = false;
            if (convertedPed.isPlayer) {
                CameraEffects.ClearCameraShake();
            }
        }
    }
}