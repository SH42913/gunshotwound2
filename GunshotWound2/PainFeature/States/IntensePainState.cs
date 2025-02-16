namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class IntensePainState : IPainState {
        private static readonly string[] MOODS = {
            "mood_drunk_1",
            "mood_sulk_1",
            "mood_injured_1",
            "shocked_1",
            "shocked_2",
        };

        public float PainThreshold => 0.6f;
        public string Color => "~o~";

        public void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            PedEffects.PlayFacialAnim(convertedPed.thisPed, "mood_injured_1", convertedPed.isMale);

            if (convertedPed.hasHandsTremor) {
                return;
            }

            if (convertedPed.isPlayer) {
                CameraEffects.ShakeCameraPermanent();
            } else {
                ref Pain pain = ref pedEntity.GetComponent<Pain>();
                float backPercent = 1f - pain.Percent();
                convertedPed.thisPed.Accuracy = (int)(backPercent * convertedPed.defaultAccuracy);
            }
        }

        public void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.hasHandsTremor) {
                return;
            }

            if (convertedPed.isPlayer) {
                CameraEffects.ClearCameraShake();
            } else {
                convertedPed.thisPed.Accuracy = convertedPed.defaultAccuracy;
            }
        }

        public bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = mainConfig.GetPainMoveSetsFor(convertedPed).intense;
            return true;
        }

        public bool TryGetMoodSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }
    }
}