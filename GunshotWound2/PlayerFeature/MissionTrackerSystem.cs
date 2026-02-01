namespace GunshotWound2.PlayerFeature {
    using System;
    using GTA;
    using GTA.UI;
    using Scellecs.Morpeh;

    public sealed class MissionTrackerSystem : ILateSystem {
        private readonly SharedData sharedData;
        private bool lastMissionActive;

        public Scellecs.Morpeh.World World { get; set; }

        public MissionTrackerSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        void IInitializer.OnAwake() { }

        public void OnUpdate(float deltaTime) {
            if (!sharedData.mainConfig.HelpTipsEnabled || Screen.IsHelpTextDisplayed) {
                return;
            }

            if (!sharedData.PlayerIsInitialized()) {
                return;
            }

            bool currentMissionActive = Game.IsMissionActive;
            if (currentMissionActive != lastMissionActive) {
                if (currentMissionActive) {
                    string key = PlayerHelpSystem.WrapKey(sharedData.mainConfig.PauseKey);
                    string message = string.Format(sharedData.localeConfig.GswPauseTip, key);
                    Screen.ShowHelpText(message, sharedData.mainConfig.HelpTipDurationInMs);
                }

                lastMissionActive = currentMissionActive;
            }
        }

        void IDisposable.Dispose() { }
    }
}