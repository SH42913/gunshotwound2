namespace GunshotWound2.PedsFeature {
    using System;
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using HealthFeature;
    using Scellecs.Morpeh;
    using Utils;

    [Serializable]
    public struct WeaponDropRequest : IRequestData {
        public EcsEntity target;
    }

    public sealed class WeaponDropSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly HashSet<EcsEntity> uniqueTargets = [];

        private Request<WeaponDropRequest> weaponDropRequest;
        private Stash<ConvertedPed> pedStash;
        private Stash<Health> healthStash;

        public EcsWorld World { get; set; }

        public WeaponDropSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            weaponDropRequest = World.GetRequest<WeaponDropRequest>();
            pedStash = World.GetStash<ConvertedPed>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            uniqueTargets.Clear();

            PlayerConfig playerConfig = sharedData.mainConfig.playerConfig;
            foreach (WeaponDropRequest request in weaponDropRequest.Consume()) {
                EcsEntity entity = request.target;
                if (entity.IsNullOrDisposed() || !uniqueTargets.Add(entity)) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity, out bool hasPed);
                if (!hasPed) {
                    continue;
                }

                if (convertedPed.isPlayer && !playerConfig.CanDropWeapon) {
                    continue;
                }

                ref Health health = ref healthStash.Get(entity, out bool hasHealth);
                if (hasHealth && health.isDead) {
                    continue;
                }

                Ped ped = convertedPed.thisPed;
                if (!ped.IsValid()) {
                    continue;
                }

#if DEBUG
                sharedData.logger.WriteInfo($"Drop weapon of {convertedPed.name}");
#endif
                ped.Weapons.Drop();
            }
        }

        void IDisposable.Dispose() { }
    }
}