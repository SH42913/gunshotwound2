namespace GunshotWound2.PlayerFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct ShowPlayerDeathReportRequest : IComponent {
        public string report;
    }

    public sealed class PlayerDeathReportSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter filter;

        public PlayerDeathReportSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public World World { get; set; }

        public void OnAwake() {
            filter = World.Filter.With<ShowPlayerDeathReportRequest>();
        }

        public void OnUpdate(float deltaTime) {
            if (!sharedData.PlayerIsInitialized()) {
                return;
            }

            foreach (Entity entity in filter) {
                ref ShowPlayerDeathReportRequest request = ref entity.GetComponent<ShowPlayerDeathReportRequest>();
                if (!string.IsNullOrEmpty(request.report)) {
                    sharedData.notifier.ShowOne(request.report, blinking: true);
                }

                entity.RemoveComponent<ShowPlayerDeathReportRequest>();
            }
        }

        void IDisposable.Dispose() { }
    }
}