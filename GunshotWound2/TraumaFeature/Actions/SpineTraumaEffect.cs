namespace GunshotWound2.TraumaFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class SpineTraumaEffect : BaseTraumaEffect {
        public override string PlayerMessage => sharedData.localeConfig.PlayerNervesCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManNervesCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanNervesCritMessage;

        public SpineTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.RequestPermanentRagdoll();
            convertedPed.hasSpineDamage = true;

            if (!convertedPed.isPlayer || sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }
        }

        public override void EveryFrame(Entity entity, ref ConvertedPed convertedPed) {
            if (!convertedPed.isRagdoll) {
                convertedPed.RequestPermanentRagdoll();
            }
        }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = false;
            convertedPed.ResetRagdoll();
        }
    }
}