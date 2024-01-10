namespace GunshotWound2.CritsFeature {
    using PedsFeature;
    using PlayerFeature;
    using Utils;

    public sealed class LungsCrit : BaseCrit {
        private const string POST_FX = "DeathFailMPIn";
        private static readonly int[] PAIN_SOUNDS = { 10, 19, 30, };

        protected override string PlayerMessage => sharedData.localeConfig.PlayerLungsCritMessage;
        protected override string ManMessage => sharedData.localeConfig.ManLungsCritMessage;
        protected override string WomanMessage => sharedData.localeConfig.WomanLungsCritMessage;

        public LungsCrit(SharedData sharedData) : base(sharedData) { }

        public override void Apply(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            CreatePain(pedEntity, 30f);
            CreateInternalBleeding(pedEntity, 1f);

            PedEffects.PlayPain(convertedPed.thisPed, sharedData.random.Next(PAIN_SOUNDS));

            if (convertedPed.isPlayer) {
                // Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, false); TODO Force sprint off
                CameraEffects.StartPostFx(POST_FX, 5000);
            }
        }

        public override void Cancel(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                // Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, false); TODO Restore sprint
                CameraEffects.StopPostFx(POST_FX);
            }
        }
    }
}