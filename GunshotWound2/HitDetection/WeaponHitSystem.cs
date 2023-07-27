namespace GunshotWound2.HitDetection {
    using System;
    using Configs;
    using GTA;
    using GTA.Native;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class WeaponHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        public WeaponHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            MainConfig config = sharedData.mainConfig;
            foreach (Scellecs.Morpeh.Entity entity in damagedPeds) {
                Ped ped = entity.GetComponent<ConvertedPed>().thisPed;
                if (!ped.IsAlive) {
                    continue;
                }

                PedHitData.WeaponTypes weaponType = default;
                if (PedWasDamagedBy(config.LightImpactHashes, ped, out uint hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.LightImpact;
                } else if (PedWasDamagedBy(config.HeavyImpactHashes, ped, out hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.HeavyImpact;
                } else if (PedWasDamagedBy(config.CuttingHashes, ped, out hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.Cutting;
                } else if (PedWasDamagedBy(config.SmallCaliberHashes, ped, out hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.SmallCaliber;
                } else if (PedWasDamagedBy(config.MediumCaliberHashes, ped, out hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.MediumCaliber;
                } else if (PedWasDamagedBy(config.HeavyCaliberHashes, ped, out hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.HeavyCaliber;
                } else if (PedWasDamagedBy(config.ShotgunHashes, ped, out hitWeapon)) {
                    weaponType = PedHitData.WeaponTypes.Shotgun;
                }

                if (weaponType == PedHitData.WeaponTypes.Nothing) {
                    sharedData.logger.WriteError("Can't detect weapon, make sure you have hash of this weapon in GSWConfig");
                } else {
                    entity.GetComponent<PedHitData>().weaponType = weaponType;
#if DEBUG
                    sharedData.logger.WriteInfo($"Weapon type is {weaponType}, weapon is {hitWeapon.ToString()}");
#endif
                }
            }
        }

        private static bool PedWasDamagedBy(uint[] hashes, Ped target, out uint weapon) {
            if (hashes != null) {
                foreach (uint hash in hashes) {
                    if (Function.Call<bool>(Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, target, hash, 0)) {
                        weapon = hash;
                        return true;
                    }
                }
            }

            weapon = default;
            return false;
        }
    }
}