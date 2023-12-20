namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using PlayerFeature;

    public sealed class LegsCrit : BaseCrit {
        private readonly int[] nmMessages = {
            169,
        };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerLegsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLegsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLegsCritMessage;

        public LegsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 20f);

            convertedPed.hasBrokenLegs = true;
            convertedPed.RequestRagdoll(3000, GTA.RagdollType.Balance);
            convertedPed.nmMessages = nmMessages;
            
            //TODO Loop?
            PedEffects.OverrideMoveRate(convertedPed.thisPed, sharedData.mainConfig.WoundConfig.MoveRateOnNervesDamage);

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSprint(false);
            }
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.hasBrokenLegs = false;
            if (convertedPed.isPlayer) {
                PlayerEffects.SetSprint(true);
            }
        }
    }
}