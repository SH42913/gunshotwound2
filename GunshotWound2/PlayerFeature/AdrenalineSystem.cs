namespace GunshotWound2.PlayerFeature {
    using System;
    using GTA;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class AdrenalineSystem : ILateSystem {
        private readonly SharedData sharedData;
        public Scellecs.Morpeh.World World { get; set; }

        public AdrenalineSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) {
            float painSlowMo = sharedData.mainConfig.playerConfig.PainSlowMo;
            if (painSlowMo is <= 0f or >= 1f) {
                return;
            }

            if (!sharedData.TryGetPlayer(out Scellecs.Morpeh.Entity playerEntity)) {
                return;
            }

            var convertedPed = playerEntity.GetComponent<ConvertedPed>();
            var pain = playerEntity.GetComponent<Pain>();
            if (!convertedPed.isPlayer || !pain.HasPain() || convertedPed.hasSpineDamage) {
                Game.TimeScale = 1f;
                return;
            }

            float painPercent = pain.Percent();
            float adjustable = 1f - painSlowMo;
            Game.TimeScale = painPercent <= 1f ? 1f - adjustable * painPercent : 1f;
        }

        void IDisposable.Dispose() {
            Game.TimeScale = 1f;
        }
    }
}