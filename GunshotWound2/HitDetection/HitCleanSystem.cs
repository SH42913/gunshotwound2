namespace GunshotWound2.HitDetection {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class HitCleanSystem : ICleanupSystem {
        private readonly SharedData sharedData;

        private Filter hits;
        private Stash<PedHitData> hitsStash;
        private Stash<ConvertedPed> pedStash;

        public HitCleanSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public World World { get; set; }

        public void OnAwake() {
            hits = World.Filter.With<PedHitData>();
            hitsStash = World.GetStash<PedHitData>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in hits) {
                if (sharedData.mainConfig.weaponConfig.CleanLastDamageFromPed) {
                    GTA.Ped ped = pedStash.Get(entity).thisPed;
                    if (ped != null && ped.Exists()) {
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