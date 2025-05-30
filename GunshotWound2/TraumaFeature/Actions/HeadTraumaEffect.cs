namespace GunshotWound2.TraumaFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class HeadTraumaEffect : BaseTraumaEffect {
        public override string PlayerMessage => null;
        public override string ManMessage => null;
        public override string WomanMessage => null;

        public HeadTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.RequestPermanentRagdoll();
            convertedPed.hasSpineDamage = true;

            if (!convertedPed.isPlayer || sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(true);
            }
        }

        public override void EveryFrame(Entity entity, ref ConvertedPed convertedPed) {
            if (!convertedPed.isRagdoll) {
                ;
                convertedPed.RequestPermanentRagdoll();
            }
        }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = false;
            convertedPed.ResetRagdoll();

            if (convertedPed.isPlayer) {
                sharedData.cameraService.SetHeadInjuryEffect(false);
            }
        }
    }
}