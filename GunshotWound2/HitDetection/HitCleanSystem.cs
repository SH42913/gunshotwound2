namespace GunshotWound2.HitDetection {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class HitCleanSystem : ICleanupSystem {
        private Filter damagedPeds;

        public World World { get; set; }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            // TODO: MoreGore compat, remove after implementation of bleeding decals
            if (!damagedPeds.IsEmpty()) {
                GTA.Script.Yield();
            }

            foreach (Entity pedEntity in damagedPeds) {
                GTA.Ped ped = pedEntity.GetComponent<ConvertedPed>().thisPed;
                if (ped != null && ped.Exists()) {
                    ped.ClearLastWeaponDamage();
                    ped.Bones.ClearLastDamaged();
                }

                pedEntity.RemoveComponent<PedHitData>();
            }
        }
    }
}