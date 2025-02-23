namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class AveragePainState : IPainState {
        private const float SHAKE_AMPLITUDE = 1f;

        private static readonly string[] MOODS = {
            "mood_stressed_1",
            "mood_frustrated_1",
            "effort_2",
            "effort_3",
        };

        public float PainThreshold => 0.3f;
        public string Color => "~y~";

        private readonly SharedData sharedData;

        public AveragePainState(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyPainIncreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.BlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(true);
                sharedData.cameraService.SetPainEffect(true);
                sharedData.cameraService.aimingShakeAmplitude += SHAKE_AMPLITUDE;
            }
        }

        public void ApplyPainDecreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(false);
                sharedData.cameraService.aimingShakeAmplitude -= SHAKE_AMPLITUDE;
                sharedData.cameraService.SetPainEffect(false);
            }
        }

        public bool TryGetMoveSets(in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = sharedData.mainConfig.GetPainMoveSetsFor(convertedPed).average;
            return true;
        }

        public bool TryGetMoodSets(in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }
    }
}