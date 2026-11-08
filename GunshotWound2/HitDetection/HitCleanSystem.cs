namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class HitCleanSystem : ICleanupSystem {
        private readonly SharedData sharedData;

        private Filter hits;
        private Stash<PedHitData> hitsStash;
        private Stash<ConvertedPed> pedStash;

        public HitCleanSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public EcsWorld World { get; set; }

        private bool CleanLastDamageFromPed => sharedData.mainConfig.weaponConfig.CleanLastDamageFromPed;

        public void OnAwake() {
            hits = World.Filter.With<PedHitData>();
            hitsStash = World.GetStash<PedHitData>();
            pedStash = World.GetStash<ConvertedPed>();
#if DEBUG
            sharedData.logger.WriteInfo($"{nameof(CleanLastDamageFromPed)}:{CleanLastDamageFromPed}");
#endif
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in hits) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity, out bool hasPed);
                if (CleanLastDamageFromPed && hasPed) {
                    Ped ped = convertedPed.thisPed;
                    if (ped.IsValid()) {
                        ped.ClearLastWeaponDamage();
                        ped.Bones.ClearLastDamaged();
                    }
                }

                hitsStash.Remove(entity);
            }
        }

        void IDisposable.Dispose() { }
    }
}