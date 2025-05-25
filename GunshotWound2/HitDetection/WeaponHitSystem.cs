namespace GunshotWound2.HitDetection {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configs;
    using GTA;
    using GTA.Math;
    using GTA.Native;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class WeaponHitSystem : ISystem {
        private const uint RAMMED_BY_CAR = 133987706;
        private const uint RUN_OVER_CAR = 2741846334;
        private const uint FALL = 3452007600;
        private const float MAX_LIGHT_IMPACT_SPEED = 10f;
        private const float MIN_FALL_SPEED = 2.5f;

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
                if (hitData.weaponType.IsValid) {
#if DEBUG
                    sharedData.logger.WriteInfo("Skip weapon detection, cause it's already detected");
#endif
                    continue;
                }

                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                Ped ped = convertedPed.thisPed;
                bool isSpecialCase;

                if (CheckWeaponHashes(ped, out uint hitWeapon, out WeaponConfig.Stats weaponType, out bool skipDamage)) {
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
                    sharedData.logger.WriteInfo("Force HeavyImpact damage after takedown");
#endif
                    weaponType = sharedData.mainConfig.weaponConfig.HeavyImpactStats;
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

        private bool CheckWeaponHashes(Ped ped, out uint hitWeapon, out WeaponConfig.Stats weaponType, out bool skipDamage) {
            WeaponConfig weaponConfig = sharedData.mainConfig.weaponConfig;
            if (PedWasDamagedBy(weaponConfig.IgnoreSet, ped, out hitWeapon)) {
#if DEBUG
                sharedData.logger.WriteInfo($"{BuildWeaponName(hitWeapon)} is ignore hash, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            weaponType = default;
            foreach (WeaponConfig.Stats stats in sharedData.mainConfig.weaponConfig.AllWeapons) {
                if (PedWasDamagedBy(stats.Hashes, ped, out hitWeapon)) {
                    weaponType = stats;
                    break;
                }
            }

            skipDamage = false;
            return weaponType.IsValid;
        }

        private bool CheckSpecialCases(ref ConvertedPed convertedPed, out WeaponConfig.Stats weaponType, out bool skipDamage) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsBeingStunned) {
#if DEBUG
                sharedData.logger.WriteInfo("It is STUN damage");
#endif
                weaponType = sharedData.mainConfig.weaponConfig.StunStats;
                skipDamage = false;
                return true;
            }

            if (ped.IsOnFire) {
#if DEBUG
                sharedData.logger.WriteInfo("It is FIRE damage, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            if (convertedPed.isPlayer && PlayerEffects.GetStaminaRemaining() >= 99f) {
#if DEBUG
                sharedData.logger.WriteInfo("It is EXHAUSTION damage, it will be skipped");
#endif
                weaponType = default;
                skipDamage = true;
                return true;
            }

            skipDamage = false;
            if (IsDamagedByWeapon(ped, RUN_OVER_CAR)) {
                weaponType = HandleRunOverCar(ped, ref skipDamage);
            } else if (ped.IsFalling || ped.IsRagdoll || IsDamagedByWeapon(ped, FALL)) {
                weaponType = HandleFalling(ped, ref skipDamage);
            } else if (IsDamagedByWeapon(ped, RAMMED_BY_CAR) || ped.IsInVehicle()) {
                weaponType = HandleCarImpact(ped);
            } else {
                sharedData.logger.WriteWarning("Unknown special case damage");
                weaponType = default;
            }

            return weaponType.IsValid;
        }

        private WeaponConfig.Stats HandleRunOverCar(Ped ped, ref bool skipDamage) {
            Vehicle possibleVehicle = GTA.World.GetClosestVehicle(ped.Position, 5f);
            Vector3 vehicleVelocity = possibleVehicle != null ? possibleVehicle.Velocity : Vector3.Zero;
            float relativeSpeed = (vehicleVelocity - ped.Velocity).Length();

#if DEBUG
            string vehicleName = possibleVehicle?.DisplayName ?? "UNKNOWN";
            sharedData.logger.WriteInfo($"It is run over car damage by {vehicleName}, relativeSpeed:{relativeSpeed}");
#endif
            if (relativeSpeed > MAX_LIGHT_IMPACT_SPEED) {
                return sharedData.mainConfig.weaponConfig.HeavyImpactStats;
            } else if (relativeSpeed < MIN_FALL_SPEED) {
                skipDamage = true;
                return default;
            } else {
                return sharedData.mainConfig.weaponConfig.LightImpactStats;
            }
        }

        private WeaponConfig.Stats HandleFalling(Ped ped, ref bool skipDamage) {
            float pedSpeed = ped.Velocity.Length();
#if DEBUG
            sharedData.logger.WriteInfo($"It is fall damage with speed {pedSpeed.ToString("F2")}");
#endif
            if (pedSpeed > MAX_LIGHT_IMPACT_SPEED) {
                return sharedData.mainConfig.weaponConfig.HeavyImpactStats;
            } else if (pedSpeed < MIN_FALL_SPEED) {
                skipDamage = true;
                return default;
            } else {
                return sharedData.mainConfig.weaponConfig.LightImpactStats;
            }
        }

        private WeaponConfig.Stats HandleCarImpact(Ped ped) {
            Vehicle vehicle = ped.CurrentVehicle;
            if (vehicle != null && vehicle.Driver == ped && !vehicle.Windows.AllWindowsIntact) {
                const float chanceToGetCut = 0.3f;
                WeaponConfig.Stats damageType = sharedData.random.IsTrueWithProbability(chanceToGetCut)
                        ? sharedData.mainConfig.weaponConfig.CuttingStats
                        : sharedData.mainConfig.weaponConfig.HeavyImpactStats;
#if DEBUG
                sharedData.logger.WriteInfo($"It is drive car impact damage: {damageType}");
#endif
                return damageType;
            } else {
#if DEBUG
                sharedData.logger.WriteInfo("It is default car impact damage");
#endif
                return sharedData.mainConfig.weaponConfig.HeavyImpactStats;
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