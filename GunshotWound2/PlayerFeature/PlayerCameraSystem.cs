namespace GunshotWound2.PlayerFeature {
    using System;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Services;

    public sealed class PlayerCameraSystem : ILateSystem {
        private readonly SharedData sharedData;

        public World World { get; set; }

        public PlayerCameraSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        void IInitializer.OnAwake() { }

        public void OnUpdate(float deltaTime) {
            CameraService cameraService = sharedData.cameraService;
            if (sharedData.TryGetPlayer(out Entity entity)) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>(out bool exist);
                cameraService.SetAimingShake(exist && convertedPed.thisPed.IsAiming);

                ref Health health = ref entity.GetComponent<Health>(out exist);
                cameraService.SetBleedingEffect(exist && health.HasBleedingWounds());
            } else {
                cameraService.SetAimingShake(false);
            }
        }

        void IDisposable.Dispose() { }
    }
}