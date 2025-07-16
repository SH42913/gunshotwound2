namespace GunshotWound2.StatusFeature.Statuses {
    using PedsFeature;
    using Utils;

    public sealed class CriticalStatus : IPedStatus {
        public const float HEALTH_THRESHOLD = 0.3f;

        private readonly SharedData sharedData;

        private static readonly string[] MOODS = {
            "mood_drunk_1",
            "mood_sulk_1",
            "mood_injured_1",
            "shocked_1",
            "shocked_2",
        };

        public string LocKey => "Status.Critical";
        public float PainThreshold => 0.7f;
        public float HealthThreshold => HEALTH_THRESHOLD;
        public Notifier.Color Color => Notifier.Color.RED;
        public float MoveRate => 0.85f;
        public string[] MaleMoveSets => sharedData.mainConfig.pedsConfig.MaleStatusMoveSets.Critical;
        public string[] FemaleMoveSets => sharedData.mainConfig.pedsConfig.FemaleStatusMoveSets.Critical;
        public string[] FacialIdleAnims => MOODS;
        public string[] PlayerSpeechSet => null;
        public string[] PedSpeechSet => null;

        public CriticalStatus(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyStatusTo(ref ConvertedPed convertedPed) {
            if (convertedPed.hasHandsTremor) {
                return;
            }

            if (convertedPed.isPlayer) {
                sharedData.cameraService.IncreaseShakeAmplitude();
            } else {
                convertedPed.thisPed.Accuracy = convertedPed.defaultAccuracy / 2;
            }
        }

        public void RemoveStatusFrom(ref ConvertedPed convertedPed) {
            if (convertedPed.hasHandsTremor) {
                return;
            }

            if (convertedPed.isPlayer) {
                sharedData.cameraService.DecreaseShakeAmplitude();
            } else {
                convertedPed.thisPed.Accuracy = convertedPed.defaultAccuracy;
            }
        }
    }
}