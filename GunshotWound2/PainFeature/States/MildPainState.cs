namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class MildPainState : IPainState {
        private static readonly string[] MOODS = {
            "effort_1",
            "mood_drivefast_1",
            "mood_angry_1",
            "mood_aiming_1",
        };

        public float PainThreshold => 0.01f;
        public string Color => "~s~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) { }
        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) { }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = mainConfig.GetPainMoveSetsFor(convertedPed).mild;
            return true;
        }

        public bool TryGetMoodSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }
    }
}