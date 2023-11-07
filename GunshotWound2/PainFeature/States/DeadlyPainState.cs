namespace GunshotWound2.PainFeature.States {
    using Peds;
    using Scellecs.Morpeh;

    public sealed class DeadlyPainState : IPainState {
        public float PainThreshold => 3f;
        public string Color => "~r~";

        public void ApplyState(Entity pedEntity, ref ConvertedPed convertedPed) {
            // if (woundedPed.IsPlayer) {
            //     woundedPed.PedHealth = Config.Data.PlayerConfig.MinimalHealth - 1;
            //     SendMessage(Locale.Data.PainShockDeath, NotifyLevels.EMERGENCY);
            // } else {
            //     woundedPed.PedHealth = -1;
            // }
        }
    }
}