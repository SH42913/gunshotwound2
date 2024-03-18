namespace GunshotWound2.PlayerFeature {
    using System;
    using Scellecs.Morpeh;

    [Serializable]
    public struct ShowPlayerDeathReportRequest : IComponent {
        public string report;
    }

    public sealed class PlayerDeathReportSystem : ILateSystem {
        private Filter filter;

        public World World { get; set; }

        public void OnAwake() {
            filter = World.Filter.With<ShowPlayerDeathReportRequest>();
        }

        public void OnUpdate(float deltaTime) {
            if (GTA.Game.IsLoading || GTA.Game.IsCutsceneActive || !GTA.Game.Player.CanControlCharacter) {
                return;
            }

            foreach (Entity entity in filter) {
                ref ShowPlayerDeathReportRequest request = ref entity.GetComponent<ShowPlayerDeathReportRequest>();
                if (!string.IsNullOrEmpty(request.report)) {
                    GTA.UI.Notification.Show(request.report, blinking: true);
                }

                entity.RemoveComponent<ShowPlayerDeathReportRequest>();
            }
        }

        void IDisposable.Dispose() { }
    }
}