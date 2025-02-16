namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class AveragePainState : IPainState {
        private const string POST_FX = "FocusIn";
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
                CameraEffects.StartPostFx(POST_FX, 5000);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.FlashAbilityBar(2000);
                PlayerEffects.SetSpecialAbilityLock(false);
                CameraEffects.StopPostFx(POST_FX);
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