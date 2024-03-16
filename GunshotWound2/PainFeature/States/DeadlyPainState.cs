namespace GunshotWound2.PainFeature.States {
    using Configs;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class DeadlyPainState : IPainState {
        public float PainThreshold => WoundConfig.DEADLY_PAIN_PERCENT;
        public string Color => "~r~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            pedEntity.GetComponent<Health>().InstantKill(sharedData.localeConfig.PainShockDeath);
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) { }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = default;
            return false;
        }
    }
}