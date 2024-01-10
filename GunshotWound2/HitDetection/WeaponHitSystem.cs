namespace GunshotWound2.HitDetection {
    using System;
    using System.Linq;
    using Configs;
    using GTA;
    using GTA.Native;
    using PedsFeature;
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
                ref PedHitData hitData = ref entity.GetComponent<PedHitData>();
                if (hitData.weaponType != PedHitData.WeaponTypes.Nothing) {
                    continue;
                }

                var isSpecial = false;
                PedHitData.WeaponTypes weaponType = default;
                Ped ped = entity.GetComponent<ConvertedPed>().thisPed;
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
                } else if (ped.IsFalling) {
                    weaponType = ped.Velocity.Length() >= 3f ? PedHitData.WeaponTypes.HeavyImpact : PedHitData.WeaponTypes.LightImpact;
                    isSpecial = true;
                } else if (ped.IsRagdoll) {
                    weaponType = PedHitData.WeaponTypes.LightImpact;
                    isSpecial = true;
                } else if (ped.IsInVehicle() && !ped.IsOnBike) {
                    weaponType = PedHitData.WeaponTypes.HeavyImpact;
                    isSpecial = true;
                }

                if (weaponType == PedHitData.WeaponTypes.Nothing) {
                    WeaponHash lastHash = ped.DamageRecords.LastOrDefault().WeaponHash;
                    sharedData.logger.WriteWarning("Can't detect weapon");
                    sharedData.logger.WriteWarning($"Make sure you have hash of this weapon(possible {lastHash}) in GSWConfig");
                } else {
                    hitData.weaponType = weaponType;
                    hitData.randomBodyPart = isSpecial;
#if DEBUG
                    string specialString = isSpecial ? "(special)" : "";
                    sharedData.logger.WriteInfo($"Weapon type is {weaponType}, weapon is {hitWeapon.ToString()}{specialString}");
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