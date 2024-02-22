namespace GunshotWound2.PainFeature.States {
    using Configs;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class DeadlyPainState : IPainState {
        public float PainThreshold => 3f;
        public string Color => "~r~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            ref Health health = ref pedEntity.GetComponent<Health>();
            health.diff -= Configs.WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);

            if (convertedPed.isPlayer) {
                sharedData.notifier.emergency.QueueMessage(sharedData.localeConfig.PainShockDeath);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) { }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = default;
            return false;
        }
    }
}