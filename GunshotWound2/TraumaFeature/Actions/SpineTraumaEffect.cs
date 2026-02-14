namespace GunshotWound2.TraumaFeature {
    using Configs;
    using GTA.NaturalMotion;
    using PedsFeature;
    using Scellecs.Morpeh;

    public class SpineTraumaEffect : BaseTraumaEffect {
        public override string PlayerMessage => sharedData.localeConfig.PlayerNervesCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManNervesCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanNervesCritMessage;

        public SpineTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Entity entity, in BodyPartConfig.BodyPart bodyPart, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = true;
            convertedPed.SetNaturalMotionBuilder(static (_, _, ped) => new BodyRelaxHelper(ped) {
                Relaxation = 0f,
                DisableJointDriving = true,
                Damping = 0f,
            }, forbidOverride: true);

            convertedPed.RequestPermanentRagdoll();

            if (!convertedPed.isPlayer || sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }
        }

        public override void Repeat(Entity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(Entity entity, ref ConvertedPed convertedPed) { }

        public override void Cancel(Entity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = false;
            convertedPed.ResetRagdoll();
        }
    }
}