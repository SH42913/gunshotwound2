namespace GunshotWound2.PlayerFeature {
    using System;
    using GTA;
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
            if (!sharedData.PlayerCanSeeNotification()) {
                return;
            }

            bool currentMissionActive = Game.IsMissionActive;
            if (currentMissionActive != lastMissionActive) {
                if (currentMissionActive) {
                    sharedData.notifier.info.QueueMessage(sharedData.localeConfig.GswPauseTip);
                }

                lastMissionActive = currentMissionActive;
            }
        }

        void IDisposable.Dispose() { }
    }
}