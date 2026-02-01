namespace GunshotWound2.PlayerFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PlayerSpeedSystem : ILateSystem {
        private const int HISTORY_LENGTH = 5;

        private readonly SharedData sharedData;

        public EcsWorld World { get; set; }

        public PlayerSpeedSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) {
            if (!sharedData.TryGetPlayer(out EcsEntity entity)) {
                return;
            }

            ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
            ref PlayerSpeedHistory history = ref entity.AddOrGetComponent<PlayerSpeedHistory>();
            history.array ??= new float[HISTORY_LENGTH];

            int newIndex = (history.lastIndex + 1) % HISTORY_LENGTH;
            history.array[newIndex] = convertedPed.thisPed.Speed;
            history.lastIndex = newIndex;

            history.max = 0f;
            for (int i = 0; i < HISTORY_LENGTH; i++) {
                history.max = Math.Max(history.array[i], history.max);
            }
        }

        void IDisposable.Dispose() { }
    }
}