namespace GunshotWound2.TraumaFeature {
    using GTA.NaturalMotion;
    using PedsFeature;
    using Scellecs.Morpeh;

    public class SpineTraumaEffect : BaseTraumaEffect {
        public override string PlayerMessage => sharedData.localeConfig.PlayerNervesCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManNervesCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanNervesCritMessage;

        public SpineTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.requestedNmHelper = new BodyRelaxHelper(convertedPed.thisPed);
            convertedPed.RequestPermanentRagdoll();
            convertedPed.hasSpineDamage = true;

            if (!convertedPed.isPlayer || sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }
        }

        public override void Repeat(Entity entity, ref ConvertedPed convertedPed) { }

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