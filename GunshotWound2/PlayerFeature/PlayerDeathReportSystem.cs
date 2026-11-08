namespace GunshotWound2.PlayerFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct ShowPlayerDeathReportRequest : IRequestData {
        public string report;
    }

    public sealed class PlayerDeathReportSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Request<ShowPlayerDeathReportRequest> deathReportRequest;

        public PlayerDeathReportSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public EcsWorld World { get; set; }

        public void OnAwake() {
            deathReportRequest = World.GetRequest<ShowPlayerDeathReportRequest>();
        }

        public void OnUpdate(float deltaTime) {
            if (!sharedData.PlayerIsInitialized()) {
                return;
            }

            ShowPlayerDeathReportRequest? last = null;
            foreach (var request in deathReportRequest.Consume()) {
                last = request;
            }

            if (!last.HasValue || string.IsNullOrEmpty(last.Value.report)) {
                return;
            }

            sharedData.notifier.HideAllLast();
            sharedData.notifier.ShowOne(last.Value.report, blinking: true);
        }

        void IDisposable.Dispose() { }
    }
}