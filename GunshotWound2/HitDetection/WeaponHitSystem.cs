﻿namespace GunshotWound2.HitDetection {
    using System;
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using GTA.Math;
    using GTA.Native;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using SHVDN;

    public sealed class WeaponHitSystem : ISystem {
        private const uint RAMMED_BY_CAR = 133987706;
        private const uint RUN_OVER_CAR = 2741846334;
        private const uint FALL = 3452007600;
        private const float MAX_LIGHT_IMPACT_SPEED = 8f;

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
            foreach (Scellecs.Morpeh.Entity entity in damagedPeds) {
                ref PedHitData hitData = ref entity.GetComponent<PedHitData>();
                if (hitData.weaponType != PedHitData.WeaponTypes.Nothing) {
                    continue;
                }

                bool isSpecialCase;
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (CheckWeaponHashes(convertedPed.thisPed, out uint hitWeapon, out PedHitData.WeaponTypes weaponType,
                                      out bool skipDamage)) {
                    isSpecialCase = false;
                } else if (CheckSpecialCases(ref convertedPed, out weaponType, out skipDamage)) {
                    isSpecialCase = true;
                } else {
                    isSpecialCase = default;
                }

                if (skipDamage) {
                    continue;
                }

                if (weaponType == PedHitData.WeaponTypes.Nothing) {
                    var damageRecord = NativeMemory.GetEntityDamageRecordEntryAtIndex(convertedPed.thisPed.MemoryAddress, 0);
                    var uintHash = unchecked((uint)damageRecord.weaponHash);
                    sharedData.logger.WriteWarning("Can't detect weapon");
                    sharedData.logger.WriteWarning($"Make sure you have hash of this weapon(possible {uintHash}) in GSWConfig");
                } else {
                    hitData.weaponType = weaponType;
                    hitData.useRandomBodyPart = isSpecialCase;
#if DEBUG
                    string specialString = isSpecialCase ? "(special)" : "";
                    sharedData.logger.WriteInfo($"Weapon type is {weaponType}, weapon is {hitWeapon.ToString()}{specialString}");
#endif
                }
            }
        }

        private bool CheckWeaponHashes(Ped ped, out uint hitWeapon, out PedHitData.WeaponTypes weaponType, out bool skipDamage) {
            if (PedWasDamagedBy(sharedData.mainConfig.IgnoreHashes, ped, out hitWeapon)) {
#if DEBUG
                sharedData.logger.WriteInfo($"{hitWeapon.ToString()} is ignore hash, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            MainConfig config = sharedData.mainConfig;
            if (PedWasDamagedBy(config.LightImpactHashes, ped, out hitWeapon)) {
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
            } else {
                weaponType = PedHitData.WeaponTypes.Nothing;
            }

            skipDamage = false;
            return weaponType != PedHitData.WeaponTypes.Nothing;
        }

        private bool CheckSpecialCases(ref ConvertedPed convertedPed, out PedHitData.WeaponTypes weaponType, out bool skipDamage) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsOnFire) {
#if DEBUG
                sharedData.logger.WriteInfo("It is FIRE damage, it will be skipped");
#endif
                weaponType = PedHitData.WeaponTypes.Nothing;
                skipDamage = true;
                return true;
            }

            if (convertedPed.isPlayer && PlayerEffects.GetStaminaRemaining() >= 99f) {
#if DEBUG
                sharedData.logger.WriteInfo("It is EXHAUSTION damage, it will be skipped");
#endif
                weaponType = PedHitData.WeaponTypes.Nothing;
                skipDamage = true;
                return true;
            }

            skipDamage = false;
            Vector3 pedVelocity = ped.Velocity;
            if (IsDamagedByWeapon(ped, RUN_OVER_CAR)) {
                Vehicle possibleVehicle = GTA.World.GetClosestVehicle(ped.Position, 5f);
                Vector3 vehicleVelocity = possibleVehicle != null ? possibleVehicle.Velocity : Vector3.Zero;
                float relativeSpeed = (vehicleVelocity - pedVelocity).Length();

#if DEBUG
                string vehicleName = possibleVehicle?.DisplayName ?? "UNKNOWN";
                sharedData.logger.WriteInfo($"It is run over car damage by {vehicleName}, relativeSpeed:{relativeSpeed}");
#endif
                if (relativeSpeed > MAX_LIGHT_IMPACT_SPEED) {
                    weaponType = PedHitData.WeaponTypes.HeavyImpact;
                } else if (relativeSpeed < 1f) {
                    weaponType = PedHitData.WeaponTypes.Nothing;
                    skipDamage = true;
                } else {
                    weaponType = PedHitData.WeaponTypes.LightImpact;
                }
            } else if (ped.IsFalling || ped.IsRagdoll || IsDamagedByWeapon(ped, FALL)) {
                float pedSpeed = pedVelocity.Length();
#if DEBUG
                sharedData.logger.WriteInfo($"It is fall damage with speed {pedSpeed.ToString("F2")}");
#endif
                weaponType = pedSpeed >= MAX_LIGHT_IMPACT_SPEED
                        ? PedHitData.WeaponTypes.HeavyImpact
                        : PedHitData.WeaponTypes.LightImpact;
            } else if (IsDamagedByWeapon(ped, RAMMED_BY_CAR) || ped.IsInVehicle()) {
#if DEBUG
                sharedData.logger.WriteInfo("It is car impact damage");
#endif
                weaponType = PedHitData.WeaponTypes.HeavyImpact;
            } else {
                sharedData.logger.WriteWarning("Unknown special case damage");
                weaponType = PedHitData.WeaponTypes.Nothing;
            }

            return weaponType != PedHitData.WeaponTypes.Nothing;
        }

        private static bool PedWasDamagedBy(HashSet<uint> hashes, Ped target, out uint weapon) {
            if (hashes != null) {
                foreach (uint hash in hashes) {
                    if (IsDamagedByWeapon(target, hash)) {
                        weapon = hash;
                        return true;
                    }
                }
            }

            weapon = default;
            return false;
        }

        private static bool IsDamagedByWeapon(Ped target, uint hash) {
            return Function.Call<bool>(Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, target, hash, 0);
        }
    }
}