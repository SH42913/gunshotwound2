namespace GunshotWound2.StatusFeature.Statuses {
    using PedsFeature;
    using Utils;

    public sealed class WarningStatus : IPedStatus {
        private static readonly string[] MOODS = {
            "effort_1",
            "mood_drivefast_1",
            "mood_angry_1",
            "mood_aiming_1",
        };

        private readonly SharedData sharedData;

        public string LocKey => "Status.Warning";
        public float PainThreshold => 0.1f;
        public float HealthThreshold => 0.85f;
        public Notifier.Color Color => Notifier.Color.YELLOW;
        public float MoveRate => 0.95f;
        public string[] MaleMoveSets => sharedData.mainConfig.pedsConfig.MaleStatusMoveSets.Warning;
        public string[] FemaleMoveSets => sharedData.mainConfig.pedsConfig.FemaleStatusMoveSets.Warning;
        public string[] FacialIdleAnims => MOODS;
        public string[] PlayerSpeechSet => null;
        public string[] PedSpeechSet => null;

        public WarningStatus(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyStatusTo(ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.IncreaseShakeAmplitude();
            }
        }

        public void RemoveStatusFrom(ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.DecreaseShakeAmplitude();
            }
        }
    }
}