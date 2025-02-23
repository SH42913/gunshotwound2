namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class IntensePainState : IPainState {
        private const float SHAKE_AMPLITUDE = 1f;
        private static readonly string[] MOODS = {
            "mood_drunk_1",
            "mood_sulk_1",
            "mood_injured_1",
            "shocked_1",
            "shocked_2",
        };

        public float PainThreshold => 0.6f;
        public string Color => "~o~";

        private readonly SharedData sharedData;

        public IntensePainState(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyPainIncreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            PedEffects.PlayFacialAnim(convertedPed.thisPed, "mood_injured_1", convertedPed.isMale);

            if (convertedPed.hasHandsTremor) {
                return;
            }

            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeAmplitude += SHAKE_AMPLITUDE;
            } else {
                ref Pain pain = ref pedEntity.GetComponent<Pain>();
                float backPercent = 1f - pain.Percent();
                convertedPed.thisPed.Accuracy = (int)(backPercent * convertedPed.defaultAccuracy);
            }
        }

        public void ApplyPainDecreased(Entity pedEntity, ref ConvertedPed convertedPed) {
            if (convertedPed.hasHandsTremor) {
                return;
            }

            if (convertedPed.isPlayer) {
                sharedData.cameraService.aimingShakeAmplitude -= SHAKE_AMPLITUDE;
            } else {
                convertedPed.thisPed.Accuracy = convertedPed.defaultAccuracy;
            }
        }

        public bool TryGetMoveSets(in ConvertedPed convertedPed, out string[] moveSets) {
            moveSets = sharedData.mainConfig.GetPainMoveSetsFor(convertedPed).intense;
            return true;
        }

        public bool TryGetMoodSets(in ConvertedPed convertedPed, out string[] moodSets) {
            moodSets = MOODS;
            return true;
        }
    }
}