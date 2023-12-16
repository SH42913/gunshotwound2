namespace GunshotWound2.Player {
    using System;
    using GTA;
    using PainFeature;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class AdrenalineSystem : ILateSystem {
        private readonly SharedData sharedData;
        public Scellecs.Morpeh.World World { get; set; }

        public AdrenalineSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) {
            float maximalSlowMo = sharedData.mainConfig.PlayerConfig.MaximalSlowMo;
            if (maximalSlowMo >= 1f || !sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                return;
            }

            var convertedPed = playerEntity.GetComponent<ConvertedPed>();
            var pain = playerEntity.GetComponent<Pain>();
            if (!convertedPed.isPlayer || pain.amount <= 0f) { //TODO || convertedPed.InPermanentRagdoll)
                Game.TimeScale = 1f;
                return;
            }

            float painPercent = pain.amount / pain.max; //TODO Move to extensions
            float adjustable = 1f - maximalSlowMo;
            Game.TimeScale = painPercent <= 1f ? 1f - adjustable * painPercent : 1f;
        }

        void IDisposable.Dispose() {
            Game.TimeScale = 1f;
        }
    }
}