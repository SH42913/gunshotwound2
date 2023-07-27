namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class HitCleanSystem : ICleanupSystem {
        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity pedEntity in damagedPeds) {
                Ped ped = pedEntity.GetComponent<ConvertedPed>().thisPed;
                if (ped != null && ped.Exists()) {
                    ped.ClearLastWeaponDamage();
                    ped.Bones.ClearLastDamaged();
                }

                pedEntity.RemoveComponent<PedHitData>();
            }
        }
    }
}