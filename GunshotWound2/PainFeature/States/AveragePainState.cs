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

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.BlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(true);
                sharedData.cameraService.SetPainEffect(true);
                sharedData.cameraService.CameraShakeAmplitude += SHAKE_AMPLITUDE;
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(false);
                sharedData.cameraService.CameraShakeAmplitude -= SHAKE_AMPLITUDE;
                sharedData.cameraService.SetPainEffect(false);
            }
        }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = mainConfig.GetPainMoveSetsFor(convertedPed).average;
            return true;
        }

        public bool TryGetMoodSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }
    }
}