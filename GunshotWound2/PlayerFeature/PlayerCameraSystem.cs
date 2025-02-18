namespace GunshotWound2.PlayerFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class PlayerCameraSystem : ILateSystem {
        private readonly SharedData sharedData;
        public World World { get; set; }

        public PlayerCameraSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        void IInitializer.OnAwake() { }

        public void OnUpdate(float deltaTime) {
            if (sharedData.TryGetPlayer(out Entity entity)) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>(out bool exist);
                sharedData.cameraService.SetAimingShake(exist && convertedPed.thisPed.IsAiming);
            } else {
                sharedData.cameraService.SetAimingShake(false);
            }
        }

        void IDisposable.Dispose() { }
    }
}