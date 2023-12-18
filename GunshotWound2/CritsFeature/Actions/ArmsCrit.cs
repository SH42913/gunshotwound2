namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class ArmsCrit : BaseCrit {
        private readonly int[] nmMessages = {
            1024,
        };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerArmsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManArmsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanArmsCritMessage;

        public ArmsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasHandsTremor = true;
            convertedPed.nmMessages = nmMessages;
            CreatePain(pedEntity, 25f);

            if (!convertedPed.isPlayer) {
                convertedPed.thisPed.Accuracy = (int)(0.1f * convertedPed.defaultAccuracy);
            } else {
                CameraEffects.ShakeCameraPermanent();
            }

            if (!convertedPed.isPlayer || sharedData.mainConfig.PlayerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
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