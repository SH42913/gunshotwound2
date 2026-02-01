namespace GunshotWound2.StatusFeature.Statuses {
    using PedsFeature;
    using PlayerFeature;
    using Utils;

    public sealed class DistressedStatus : IPedStatus {
        private static readonly string[] MOODS = {
            "mood_stressed_1",
            "mood_frustrated_1",
            "effort_2",
            "effort_3",
        };

        private readonly SharedData sharedData;

        public string LocKey => "Status.Distressed";
        public float PainThreshold => 0.4f;
        public float HealthThreshold => 0.6f;
        public Notifier.Color Color => Notifier.Color.ORANGE;
        public float MoveRate => 0.9f;
        public string[] MaleMoveSets => sharedData.mainConfig.pedsConfig.MaleStatusMoveSets.Distressed;
        public string[] FemaleMoveSets => sharedData.mainConfig.pedsConfig.FemaleStatusMoveSets.Distressed;
        public string[] FacialIdleAnims => MOODS;
        public string[] PlayerSpeechSet => null;
        public string[] PedSpeechSet => null;

        public DistressedStatus(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ApplyStatusTo(ref ConvertedPed convertedPed) {
            convertedPed.BlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(true);
                sharedData.cameraService.SetPainEffect(true);
                sharedData.cameraService.IncreaseShakeAmplitude();
            }
        }

        public void RemoveStatusFrom(ref ConvertedPed convertedPed) {
            convertedPed.UnBlockSprint();

            if (convertedPed.isPlayer) {
                PlayerEffects.SetSpecialAbilityLock(false);
                sharedData.cameraService.SetPainEffect(false);
                sharedData.cameraService.DecreaseShakeAmplitude();
            }
        }
    }
}