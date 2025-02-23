namespace GunshotWound2.PainFeature.States {
    using Configs;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class DeadlyPainState : IPainState {
        public float PainThreshold => WoundConfig.DEADLY_PAIN_PERCENT;
        public string Color => "~r~";

        private readonly SharedData sharedData;

        public DeadlyPainState(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyPainIncreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            pedEntity.GetComponent<Health>().InstantKill(sharedData.localeConfig.PainShockDeath);
        }

        public void ApplyPainDecreased(Entity pedEntity, ref ConvertedPed convertedPed) { }

        public bool TryGetMoveSets(in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = null;
            return false;
        }

        public bool TryGetMoodSets(in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = null;
            return false;
        }
    }
}