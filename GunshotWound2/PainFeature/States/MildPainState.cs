namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class MildPainState : IPainState {
        private const float SHAKE_AMPLITUDE = 1f;

        private static readonly string[] MOODS = {
            "effort_1",
            "mood_drivefast_1",
            "mood_angry_1",
            "mood_aiming_1",
        };

        public float PainThreshold => 0.01f;
        public string Color => "~s~";

        private readonly SharedData sharedData;

        public MildPainState(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyPainIncreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeAmplitude += SHAKE_AMPLITUDE;
            }
        }

        public void ApplyPainDecreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeAmplitude -= SHAKE_AMPLITUDE;
            }
        }

        public bool TryGetMoveSets(in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = sharedData.mainConfig.GetPainMoveSetsFor(convertedPed).mild;
            return true;
        }

        public bool TryGetMoodSets(in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }
    }
}