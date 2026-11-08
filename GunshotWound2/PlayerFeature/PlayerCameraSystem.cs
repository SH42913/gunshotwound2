namespace GunshotWound2.PlayerFeature {
    using System;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Services;

    public sealed class PlayerCameraSystem : ILateSystem {
        private readonly SharedData sharedData;

        private bool wasHeavyBleed;

        public EcsWorld World { get; set; }

        public PlayerCameraSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        void IInitializer.OnAwake() { }

        public void OnUpdate(float deltaTime) {
            CameraService cameraService = sharedData.cameraService;
            if (sharedData.TryGetPlayer(out EcsEntity entity)) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>(out bool exist);
                cameraService.aimingShakeMult = sharedData.mainConfig.playerConfig.AimShakingMult;
                cameraService.SetAimingShake(exist && convertedPed.thisPed.IsAiming);

                float total = HealthFeature.CalculateSeverityOfAllBleedingWounds(entity);
                float deadlyThreshold = entity.GetComponent<Health>().CalculateDeadlyBleedingThreshold(convertedPed);
                bool isHeavyBleed = total >= deadlyThreshold;
                if (wasHeavyBleed != isHeavyBleed) {
                    cameraService.SetHeavyBleedingEffect(isHeavyBleed);
                    wasHeavyBleed = isHeavyBleed;
                }
            } else {
                cameraService.SetAimingShake(false);
            }
        }

        void IDisposable.Dispose() { }
    }
}