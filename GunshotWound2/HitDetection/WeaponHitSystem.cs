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
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class WeaponHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;
        private Stash<PedHitData> hitsStash;
        private Stash<ConvertedPed> pedsStash;

        public EcsWorld World { get; set; }

        private WeaponConfig WeaponConfig => sharedData.mainConfig.weaponConfig;

        public WeaponHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
            hitsStash = World.GetStash<PedHitData>();
            pedsStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in damagedPeds) {
                ref PedHitData hitData = ref hitsStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedsStash.Get(entity);
                ProcessHit(ref hitData, ref convertedPed);
            }
        }

        private void ProcessHit(ref PedHitData hitData, ref ConvertedPed convertedPed) {
            if (hitData.weaponType.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip weapon detection, cause it's already detected");
#endif
                return;
            }

            Ped ped = convertedPed.thisPed;
            bool hasLastDamage = PedEffects.TryGetLastDamageRecord(ped,
                                                                   out uint weaponHash,
                                                                   out int attackerHandle,
                                                                   out int time);

            int damageTimeDiff = Game.GameTime - time;
            bool isValidLastDamage = hasLastDamage && damageTimeDiff <= 20;
#if DEBUG
            if (hasLastDamage) {
                string name = convertedPed.name;
                sharedData.logger.WriteInfo($"Record {name} A:{attackerHandle} W:{weaponHash}, time={time}({damageTimeDiff})");
            }
#endif

            if (CheckIgnoreSet(ped, ref weaponHash)) {
                return;
            }

            WeaponConfig.Weapon weaponType = default;
            if (convertedPed.isPlayer) {
                if (!isValidLastDamage) {
                    weaponHash = 0;
                }

                weaponType = DetectWeaponType(ped, ref weaponHash);
            } else if (isValidLastDamage) {
                RefreshAggressor(ref convertedPed, attackerHandle);
                weaponType = DetectWeaponType(ped, ref weaponHash);
            }

            var defaultDamage = false;
            var isSpecialCase = false;
            if (!weaponType.IsValid) {
                weaponType = CheckSpecialCases(ref convertedPed, out defaultDamage);
                isSpecialCase = weaponType.IsValid;
            }

            if (defaultDamage) {
#if DEBUG
                sharedData.logger.WriteInfo($"It is default damage, so skip it");
#endif
                return;
            }

            if (weaponType.IsValid) {
                hitData.weaponHash = weaponHash;
                hitData.weaponType = weaponType;
                hitData.hits = 1;
                hitData.useRandomBodyPart = isSpecialCase;
#if DEBUG
                string weaponName = BuildWeaponName(weaponHash);
                string specialString = isSpecialCase ? "(special)" : "";
                string name = convertedPed.name;
                sharedData.logger.WriteInfo($"Weapon type is {weaponType.Key}, {weaponName}{specialString} damaged {name}");
#endif
            } else {
                sharedData.logger.WriteWarning("Can't detect weapon!");
                PedEffects.TryGetLastDamageRecord(ped, out uint uintHash, out _, out int gameTime);
                int timeDiff = Game.GameTime - gameTime;
                sharedData.logger.WriteWarning($"Last record - {BuildWeaponName(uintHash)}, {timeDiff} frames ago");
            }
        }

        private void RefreshAggressor(ref ConvertedPed convertedPed, int attackerHandle) {
            if (PedEffects.TryGetPedByHandle(attackerHandle, out Ped aggressor)) {
#if DEBUG
                sharedData.logger.WriteInfo($"Confirmed aggressor for {convertedPed.name} is {aggressor.Handle}");
#endif
                convertedPed.lastAggressor = aggressor;
            } else {
                convertedPed.lastAggressor = null;
            }
        }

        private bool CheckIgnoreSet(Ped ped, ref uint weaponHash) {
            bool directCheck = weaponHash != 0 && WeaponConfig.IgnoreSet.Contains(weaponHash);
            if (directCheck) {
#if DEBUG
                sharedData.logger.WriteInfo($"{BuildWeaponName(weaponHash)} is ignore hash, it will be skipped");
#endif
                return true;
            }

            bool bruteCheck = PedWasDamagedBy(WeaponConfig.IgnoreSet, ped, out uint hitWeapon);
            if (bruteCheck) {
                weaponHash = hitWeapon;
#if DEBUG
                sharedData.logger.WriteInfo($"{BuildWeaponName(weaponHash)} is ignore hash, it will be skipped (brute)");
#endif
                return true;
            } else {
                return false;
            }
        }

        private WeaponConfig.Weapon DetectWeaponType(Ped ped, ref uint weaponHash) {
            foreach (WeaponConfig.Weapon weapon in WeaponConfig.Weapons) {
                if (weapon.Hashes == null || weapon.Hashes.Count < 1) {
                    continue;
                }

                bool directCheck = weaponHash != 0 && weapon.Hashes.Contains(weaponHash);
                if (directCheck) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Hash {weaponHash} was directly found in {weapon.Key}");
#endif
                    return weapon;
                }
            }

            foreach (WeaponConfig.Weapon weapon in WeaponConfig.Weapons) {
                bool bruteCheck = PedWasDamagedBy(weapon.Hashes, ped, out uint hitWeapon);
                if (bruteCheck) {
                    weaponHash = hitWeapon;
#if DEBUG
                    sharedData.logger.WriteInfo($"Hash {weaponHash} was found with brute in {weapon.Key}");
#endif
                    return weapon;
                }
            }

            return default;
        }

        private WeaponConfig.Weapon CheckSpecialCases(ref ConvertedPed convertedPed, out bool skipDamage) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsBeingStunned) {
#if DEBUG
                sharedData.logger.WriteInfo("It is STUN damage");
#endif
                skipDamage = false;
                return WeaponConfig.Stun;
            }

            if (ped.IsOnFire || IsDamagedByWeapon(ped, WeaponConfig.WEAPON_FIRE)) {
#if DEBUG
                sharedData.logger.WriteInfo("It is FIRE damage, it will be skipped");
#endif
                skipDamage = true;
                return default;
            }

            if (convertedPed.isPlayer && PlayerEffects.GetStaminaRemaining() >= 99f
                || IsDamagedByWeapon(ped, WeaponConfig.WEAPON_EXHAUSTION)) {
#if DEBUG
                sharedData.logger.WriteInfo("It is EXHAUSTION damage, it will be skipped");
#endif
                skipDamage = true;
                return default;
            }

            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_RUN_OVER_BY_CAR)) {
                return HandleRunOverCar(ped, out skipDamage);
            }

            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_FALL) || ped.IsFalling || ped.IsRagdoll) {
                return HandleFalling(ped, out skipDamage);
            }

            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_RAMMED_BY_CAR)) {
                skipDamage = false;
                return HandleCarImpact(ped);
            }

            skipDamage = false;
            return default;
        }

        private WeaponConfig.Weapon HandleRunOverCar(Ped ped, out bool skipDamage) {
            Vehicle possibleVehicle = GTA.World.GetClosestVehicle(ped.Position, 5f);
            Vector3 vehicleVelocity = possibleVehicle != null ? possibleVehicle.Velocity : Vector3.Zero;
            float relativeSpeed = (vehicleVelocity - ped.Velocity).Length();

#if DEBUG
            string vehicleName = possibleVehicle?.DisplayName ?? "UNKNOWN";
            sharedData.logger.WriteInfo($"It is run over car damage by {vehicleName}, relativeSpeed:{relativeSpeed}");
#endif
            if (relativeSpeed > WeaponConfig.HardRunOverThreshold) {
                skipDamage = false;
                return WeaponConfig.HardRunOverCar;
            } else if (relativeSpeed < WeaponConfig.LightRunOverThreshold) {
                skipDamage = true;
                return default;
            } else {
                skipDamage = false;
                return WeaponConfig.LightRunOverCar;
            }
        }

        private WeaponConfig.Weapon HandleFalling(Ped ped, out bool skipDamage) {
            float pedSpeed = ped.Velocity.Length();
#if DEBUG
            sharedData.logger.WriteInfo($"It is fall damage with speed {pedSpeed.ToString("F2")}");
#endif
            if (pedSpeed > WeaponConfig.HardFallThreshold) {
                skipDamage = false;
                return WeaponConfig.HardFall;
            } else if (pedSpeed < WeaponConfig.LightFallThreshold) {
                skipDamage = true;
                return default;
            } else {
                skipDamage = false;
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