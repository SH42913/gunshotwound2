namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class SpineCrit : BaseCrit {
        protected override string CritName => sharedData.localeConfig.NervesCrit;
        protected override string PlayerMessage => sharedData.localeConfig.PlayerNervesCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManNervesCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanNervesCritMessage;

        public SpineCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.ResetRagdoll();
            convertedPed.RequestPermanentRagdoll();
            convertedPed.hasSpineDamage = true;

            if (!convertedPed.isPlayer || sharedData.mainConfig.PlayerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }
        }

        public override void EveryFrame(Entity pedEntity, ref ConvertedPed convertedPed) {
            if (!convertedPed.isRagdoll) {
                convertedPed.RequestPermanentRagdoll();
            }
        }

        public override void Cancel(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = false;
            convertedPed.ResetRagdoll();
        }
    }
}