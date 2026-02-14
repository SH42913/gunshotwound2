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
    using Utils;
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
                ProcessHit(entity, ref hitData, ref convertedPed);
            }
        }

        private void ProcessHit(EcsEntity entity, ref PedHitData hitData, ref ConvertedPed convertedPed) {
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
            string name = convertedPed.name;
            if (!hasLastDamage) {
                sharedData.logger.WriteWarning($"Ped {name} has damage, but doesn't have damage record");
            }
#if DEBUG
            else if (!isValidLastDamage) {
                sharedData.logger.WriteInfo($"Ped {name} has damage, but damage record is outdated({damageTimeDiff.ToString()})");
            } else {
                sharedData.logger.WriteInfo($"Record {name} A:{attackerHandle} W:{weaponHash}, time={time}({damageTimeDiff})");
            }
#endif

            if (CheckIgnoreSet(ped, ref weaponHash)) {
                return;
            }

            WeaponConfig.Weapon weaponType = default;
            if (isValidLastDamage) {
                weaponType = DetectWeaponTypeDirectly(weaponHash);
            }

            var specialHitCount = 0;
            var isSpecialCase = false;
            var skipAsDefaultDamage = false;
            if (!weaponType.IsValid) {
                weaponType = CheckSpecialCases(entity, ref convertedPed, out specialHitCount);
                isSpecialCase = weaponType.IsValid;
                skipAsDefaultDamage = specialHitCount < 1;
            }

            if (skipAsDefaultDamage) {
#if DEBUG
                sharedData.logger.WriteInfo("It is default damage, so skip it");
#endif
                return;
            }

            if (!weaponType.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Need to try find weapon with brute");
#endif
                weaponType = DetectWeaponTypeBrute(ped, out weaponHash);
            }

            if (isValidLastDamage) {
                RefreshAggressor(ref convertedPed, ref hitData, attackerHandle);
            }

            if (weaponType.IsValid) {
                hitData.weaponHash = weaponHash;
                hitData.weaponType = weaponType;
                hitData.hits = isSpecialCase ? specialHitCount : 1;
                hitData.useRandomBodyPart = isSpecialCase;
#if DEBUG
                string weaponName = BuildWeaponName(weaponHash);
                string specialString = isSpecialCase ? "(special)" : "";
                sharedData.logger.WriteInfo($"Weapon type is {weaponType.Key}, {weaponName}{specialString} damaged {name}");
#endif
            } else {
                sharedData.logger.WriteWarning("Can't detect weapon!");
                PedEffects.TryGetLastDamageRecord(ped, out uint uintHash, out _, out int gameTime);
                int timeDiff = Game.GameTime - gameTime;
                sharedData.logger.WriteWarning($"Last record - {BuildWeaponName(uintHash)}, {timeDiff} frames ago");
                sharedData.notifier.ShowOne(sharedData.localeConfig.GswCantDetectWeapon, blinking: true, Notifier.Color.RED);
            }
        }

        private void RefreshAggressor(ref ConvertedPed convertedPed, ref PedHitData hitData, int attackerHandle) {
            if (PedEffects.TryGetPedByHandle(attackerHandle, out Ped aggressor)) {
#if DEBUG
                sharedData.logger.WriteInfo($"Confirmed aggressor for {convertedPed.name} is {aggressor.Handle}");
#endif
                hitData.aggressor = aggressor;
            } else {
                hitData.aggressor = null;
            }

            convertedPed.lastAggressor = aggressor;
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

        private WeaponConfig.Weapon DetectWeaponTypeDirectly(uint weaponHash) {
            foreach (WeaponConfig.Weapon weapon in WeaponConfig.Weapons) {
                if (weapon.Hashes == null || weapon.Hashes.Count < 1) {
                    continue;
                }

                if (weapon.Hashes.Contains(weaponHash)) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Hash {weaponHash} was directly found in {weapon.Key}");
#endif
                    return weapon;
                }
            }

            return default;
        }

        private WeaponConfig.Weapon DetectWeaponTypeBrute(Ped ped, out uint weaponHash) {
            foreach (WeaponConfig.Weapon weapon in WeaponConfig.Weapons) {
                if (PedWasDamagedBy(weapon.Hashes, ped, out uint hitWeapon)) {
                    weaponHash = hitWeapon;
#if DEBUG
                    sharedData.logger.WriteInfo($"Hash {hitWeapon} was found with brute in {weapon.Key}");
#endif
                    return weapon;
                }
            }

            weaponHash = 0;
            return default;
        }

        private WeaponConfig.Weapon CheckSpecialCases(EcsEntity entity, ref ConvertedPed convertedPed, out int hitCount) {
            Ped ped = convertedPed.thisPed;
            if (ped.IsBeingStunned) {
#if DEBUG
                sharedData.logger.WriteInfo("It is STUN damage");
#endif
                hitCount = 1;
                return WeaponConfig.Stun;
            }

            if (ped.IsOnFire || IsDamagedByWeapon(ped, WeaponConfig.WEAPON_FIRE)) {
#if DEBUG
                sharedData.logger.WriteInfo("It is FIRE damage, it will be skipped");
#endif
                hitCount = 0;
                return default;
            }

            if (convertedPed.isPlayer && PlayerEffects.GetStaminaRemaining() >= 99f
                || IsDamagedByWeapon(ped, WeaponConfig.WEAPON_EXHAUSTION)) {
#if DEBUG
                sharedData.logger.WriteInfo("It is EXHAUSTION damage, it will be skipped");
#endif
                hitCount = 0;
                return default;
            }

            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_RAMMED_BY_CAR)) {
                return HandleCarImpact(entity, convertedPed, out hitCount);
            }

            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_RUN_OVER_BY_CAR)) {
                return HandleRunOverCar(ped, out hitCount);
            }

            if (IsDamagedByWeapon(ped, WeaponConfig.WEAPON_FALL) || ped.IsFalling || ped.IsRagdoll) {
                return HandleFalling(ped.Velocity.Length(), out hitCount);
            }

            hitCount = 1;
            return default;
        }

        private WeaponConfig.Weapon HandleRunOverCar(Ped ped, out int hitCount) {
            Vehicle possibleVehicle = GTA.World.GetClosestVehicle(ped.Position, 4f);
            if (possibleVehicle == null) {
#if DEBUG
                sharedData.logger.WriteInfo("Run over car damage, but no vehicle, treat like falling");
#endif
                return HandleFalling(ped.Velocity.Length(), out hitCount);
            }

            float relativeSpeed = (possibleVehicle.Velocity - ped.Velocity).Length();
#if DEBUG
            string vehicleName = possibleVehicle.DisplayName;
            float mass = possibleVehicle.HandlingData.Mass;
            sharedData.logger.WriteInfo($"It is run over car damage by {vehicleName}, relativeSpeed:{relativeSpeed}, mass:{mass}");
#endif

            float hardHitMult = relativeSpeed / WeaponConfig.HardRunOverThreshold;
            if (hardHitMult > 1f) {
                float massMult = possibleVehicle.HandlingData.Mass / WeaponConfig.HardRunOverVehMassReference;
#if DEBUG
                sharedData.logger.WriteInfo($"HardHitMult:{hardHitMult}, MassMult:{massMult}");
#endif
                hitCount = (int)Math.Round(hardHitMult * massMult);
                if (hitCount < 1) {
                    hitCount = 1;
                }

                return WeaponConfig.HardRunOverCar;
            } else if (relativeSpeed < WeaponConfig.LightRunOverThreshold) {
                hitCount = 0;
                return default;
            } else {
                hitCount = 1;
                return WeaponConfig.LightRunOverCar;
            }
        }

        private WeaponConfig.Weapon HandleFalling(float pedSpeed, out int hitCount) {
            float hardFallMult = pedSpeed / WeaponConfig.HardFallThreshold;
#if DEBUG
            sharedData.logger.WriteInfo($"It is fall damage with speed {pedSpeed}, hardMult:{hardFallMult}");
#endif
            if (hardFallMult > 1f) {
                hitCount = (int)Math.Round(hardFallMult);
                return WeaponConfig.HardFall;
            } else if (pedSpeed < WeaponConfig.LightFallThreshold) {
                hitCount = 0;
                return default;
            } else {
                hitCount = 1;
                return WeaponConfig.LightFall;
            }
        }

        private WeaponConfig.Weapon HandleCarImpact(EcsEntity entity, in ConvertedPed convertedPed, out int hitCount) {
            if (convertedPed.isPlayer) {
                float maxSpeed = entity.GetComponent<PlayerSpeedHistory>().max;
                float hardMult = maxSpeed / sharedData.mainConfig.weaponConfig.CarCrashReferenceSpeed;
#if DEBUG
                sharedData.logger.WriteInfo($"It is car crash damage at speed {maxSpeed}, hardMult:{hardMult}");
#endif

                hitCount = (int)Math.Round(hardMult);
                if (hitCount < 1) {
                    hitCount = 1;
                }

                return WeaponConfig.CarCrash;
            } else {
#if DEBUG
                sharedData.logger.WriteWarning("It is car crash damage, but ped is not player");
#endif
                hitCount = 1;
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