namespace GunshotWound2.HitDetection {
    using System;
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using GTA.Math;
    using GTA.Native;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;

    public sealed class WeaponHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        private WeaponConfig WeaponConfig => sharedData.mainConfig.weaponConfig;

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
                if (hitData.weaponType.IsValid) {
#if DEBUG
                    sharedData.logger.WriteInfo("Skip weapon detection, cause it's already detected");
#endif
                    continue;
                }

                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                Ped ped = convertedPed.thisPed;
                bool isSpecialCase;

                if (CheckWeaponHashes(ped, out uint hitWeapon, out WeaponConfig.Weapon weaponType, out bool skipDamage)) {
                    isSpecialCase = false;
                } else if (CheckSpecialCases(ref convertedPed, out weaponType, out skipDamage)) {
                    isSpecialCase = true;
                } else {
                    isSpecialCase = false;
                }

                if (skipDamage) {
                    continue;
                }

                if (hitData.afterTakedown) {
#if DEBUG
                    sharedData.logger.WriteInfo("Set special weapon for takedown");
#endif
                    weaponType = WeaponConfig.Takedown;
                }

                if (!weaponType.IsValid) {
                    sharedData.logger.WriteWarning("Can't detect weapon!");
                    PedEffects.TryGetLastDamageRecord(ped, out uint uintHash, out _, out int gameTime);
                    int timeDiff = Game.GameTime - gameTime;
                    sharedData.logger.WriteWarning($"Possible hash - {BuildWeaponName(uintHash)}, {timeDiff} frames ago");
                    sharedData.logger.WriteWarning("Make sure you have this weapon in Weapons section of GunshotWound2.Weapons.xml");
                } else {
                    hitData.weaponHash = hitWeapon;
                    hitData.weaponType = weaponType;
                    hitData.useRandomBodyPart = isSpecialCase;
#if DEBUG
                    string weaponName = BuildWeaponName(hitWeapon);
                    string specialString = isSpecialCase ? "(special)" : "";
                    sharedData.logger.WriteInfo($"Weapon type is {weaponType.Key}, weapon is {weaponName}{specialString}");
#endif
                }
            }
        }

        private bool CheckWeaponHashes(Ped ped, out uint hitWeapon, out WeaponConfig.Weapon weaponType, out bool skipDamage) {
            WeaponConfig weaponConfig = WeaponConfig;
            if (PedWasDamagedBy(weaponConfig.IgnoreSet, ped, out hitWeapon)) {
#if DEBUG
                sharedData.logger.WriteInfo($"{BuildWeaponName(hitWeapon)} is ignore hash, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            weaponType = default;
            foreach (WeaponConfig.Weapon stats in WeaponConfig.Weapons) {
                if (PedWasDamagedBy(stats.Hashes, ped, out hitWeapon)) {
                    weaponType = stats;
                    break;
                }
            }

            skipDamage = false;
            return weaponType.IsValid;
        }

        private bool CheckSpecialCases(ref ConvertedPed convertedPed, out WeaponConfig.Weapon weaponType, out bool skipDamage) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsBeingStunned) {
#if DEBUG
                sharedData.logger.WriteInfo("It is STUN damage");
#endif
                weaponType = WeaponConfig.Stun;
                skipDamage = false;
                return true;
            }

            if (ped.IsOnFire || IsDamagedByWeapon(ped, WeaponConfig.WEAPON_FIRE)) {
#if DEBUG
                sharedData.logger.WriteInfo("It is FIRE damage, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            if (convertedPed.isPlayer && PlayerEffects.GetStaminaRemaining() >= 99f
                || IsDamagedByWeapon(ped, WeaponConfig.WEAPON_EXHAUSTION)) {
#if DEBUG
                sharedData.logger.WriteInfo("It is EXHAUSTION damage, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            skipDamage = false;
            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_RUN_OVER_BY_CAR)) {
                weaponType = HandleRunOverCar(ped, ref skipDamage);
            } else if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_FALL) || ped.IsFalling || ped.IsRagdoll) {
                weaponType = HandleFalling(ped, ref skipDamage);
            } else if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_RAMMED_BY_CAR)) {
                weaponType = HandleCarImpact(ped);
            } else {
                sharedData.logger.WriteWarning("Unknown special case damage");
                weaponType = default;
            }

            return weaponType.IsValid;
        }

        private WeaponConfig.Weapon HandleRunOverCar(Ped ped, ref bool skipDamage) {
            Vehicle possibleVehicle = GTA.World.GetClosestVehicle(ped.Position, 5f);
            Vector3 vehicleVelocity = possibleVehicle != null ? possibleVehicle.Velocity : Vector3.Zero;
            float relativeSpeed = (vehicleVelocity - ped.Velocity).Length();

#if DEBUG
            string vehicleName = possibleVehicle?.DisplayName ?? "UNKNOWN";
            sharedData.logger.WriteInfo($"It is run over car damage by {vehicleName}, relativeSpeed:{relativeSpeed}");
#endif
            if (relativeSpeed > WeaponConfig.HardRunOverThreshold) {
                return WeaponConfig.HardRunOverCar;
            } else if (relativeSpeed < WeaponConfig.LightRunOverThreshold) {
                skipDamage = true;
                return default;
            } else {
                return WeaponConfig.LightRunOverCar;
            }
        }

        private WeaponConfig.Weapon HandleFalling(Ped ped, ref bool skipDamage) {
            float pedSpeed = ped.Velocity.Length();
#if DEBUG
            sharedData.logger.WriteInfo($"It is fall damage with speed {pedSpeed.ToString("F2")}");
#endif
            if (pedSpeed > WeaponConfig.HardFallThreshold) {
                return WeaponConfig.HardFall;
            } else if (pedSpeed < WeaponConfig.LightFallThreshold) {
                skipDamage = true;
                return default;
            } else {
                return WeaponConfig.LightFall;
            }
        }

        private WeaponConfig.Weapon HandleCarImpact(Ped ped) {
            Vehicle vehicle = ped.CurrentVehicle;
            if (vehicle != null) {
#if DEBUG
                sharedData.logger.WriteInfo($"It is car crash damage at speed {vehicle.Speed}");
#endif
                return WeaponConfig.CarCrash;
            } else {
#if DEBUG
                sharedData.logger.WriteInfo("It is car crash damage, but ped is not in vehicle");
#endif
                return WeaponConfig.CarCrash;
            }
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

            weapon = 0;
            return false;
        }

        private static bool IsDamagedByWeapon(Ped target, uint hash) {
            return Function.Call<bool>(Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, target, hash, 0);
        }

        private static string BuildWeaponName(uint hash) {
            return WeaponConfig.BuildWeaponName(hash);
        }
    }
}