namespace GunshotWound2.TraumaFeature {
    using Configs;
    using GTA;
    using GTA.NaturalMotion;
    using PedsFeature;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;

    public class SpineTraumaEffect : BaseTraumaEffect {
        public override string PlayerMessage => sharedData.localeConfig.PlayerNervesCritMessage;
        public override string ManMessage => sharedData.localeConfig.ManNervesCritMessage;
        public override string WomanMessage => sharedData.localeConfig.WomanNervesCritMessage;

        public SpineTraumaEffect(SharedData sharedData) : base(sharedData) { }

        public override void Apply(EcsEntity entity, in BodyPartConfig.BodyPart bodyPart, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = true;
            convertedPed.SetNaturalMotionBuilder(GetNMBuilder(), forbidOverride: true);
            convertedPed.RequestPermanentRagdoll();

            if (!convertedPed.isPlayer || sharedData.mainConfig.playerConfig.CanDropWeapon) {
                convertedPed.thisPed.Weapons.Drop();
            }

            Ped ped = convertedPed.thisPed;
            if (PedEffects.OnAnyBike(ped)) {
                PedEffects.KnockOffVehicle(ped);
            } else if (PedEffects.InAnyVehicle(ped, atGetIn: false)) {
                GTAHelpers.PlayDeathAnimationInVehicle(ped);
            }
        }

        public override void Repeat(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void EveryFrame(EcsEntity entity, ref ConvertedPed convertedPed) { }

        public override void Cancel(EcsEntity entity, ref ConvertedPed convertedPed) {
            convertedPed.hasSpineDamage = false;
            convertedPed.ResetRagdoll();
        }

        protected virtual ConvertedPed.NaturalMotionBuilder GetNMBuilder() {
            return static (_, _, ped) => new BodyRelaxHelper(ped) {
                Relaxation = 100f,
                DisableJointDriving = true,
                Damping = 0f,
            };
        }
    }
}