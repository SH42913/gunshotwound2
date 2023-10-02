﻿namespace GunshotWound2.Damage {
    using System;
    using System.Collections.Generic;
    using Configs;
    using HealthCare;
    using HitDetection;
    using Peds;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class WoundSystem : ISystem {
        private readonly SharedData sharedData;
        private readonly Dictionary<PedHitData.WeaponTypes, BaseWeaponDamage> weaponDamages;

        private Filter damagedPeds;

        public World World { get; set; }

        public WoundSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            weaponDamages = new Dictionary<PedHitData.WeaponTypes, BaseWeaponDamage>(7) {
                { PedHitData.WeaponTypes.LightImpact, new LightImpactDamage(sharedData) },
                { PedHitData.WeaponTypes.HeavyImpact, new HeavyImpactDamage(sharedData) },
                { PedHitData.WeaponTypes.Cutting, new CuttingDamage(sharedData) },
                { PedHitData.WeaponTypes.SmallCaliber, new SmallCaliberDamage(sharedData) },
                { PedHitData.WeaponTypes.MediumCaliber, new MediumCaliberDamage(sharedData) },
                { PedHitData.WeaponTypes.HeavyCaliber, new HeavyCaliberDamage(sharedData) },
                { PedHitData.WeaponTypes.Shotgun, new ShotgunDamage(sharedData) },
            };
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>().With<Health>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Entity pedEntity in damagedPeds) {
                ref PedHitData hitData = ref pedEntity.GetComponent<PedHitData>();
                if (hitData.weaponType == PedHitData.WeaponTypes.Nothing) {
                    continue;
                }

                if (weaponDamages.TryGetValue(hitData.weaponType, out BaseWeaponDamage weaponDamage)) {
                    ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();

                    WoundData? wound = weaponDamage.ProcessHit(convertedPed.thisPed, hitData);
#if DEBUG
                    sharedData.logger.WriteInfo(wound.HasValue ? $"New wound {wound.Value.ToString()} " : "No wound created");
#endif
                    ProcessWound(pedEntity, ref hitData, ref convertedPed, ref wound);
                } else {
                    sharedData.logger.WriteWarning($"Doesn't support {hitData.weaponType}");
                }
            }
        }

        private void ProcessWound(Entity pedEntity, ref PedHitData hitData, ref ConvertedPed convertedPed, ref WoundData? wound) {
            if (!wound.HasValue) {
                return;
            }

            WoundData woundData = wound.Value;
            CreateDamage(pedEntity, woundData.Damage, ref hitData);
            CreateBleeding(pedEntity, woundData.BleedSeverity);
            CreatePain(pedEntity, woundData.Pain);
            CreateCrit(pedEntity, woundData.HasCrits);

            if (woundData.ArterySevered) {
                // CreateBleeding(woundedPed, woundData.BleedSeverity, 1f, _locale.Data.SeveredArtery);
            }

            if (convertedPed.isPlayer) {
                SendWoundInfo(woundData);
            }
        }

        private void CreateDamage(Entity pedEntity, float damage, ref PedHitData hitData) {
            damage -= hitData.healthDiff;
            if (damage <= 0f) {
                return;
            }

            float deviation = sharedData.mainConfig.WoundConfig.DamageDeviation;
            float mult = sharedData.mainConfig.WoundConfig.DamageMultiplier;

            ref Health health = ref pedEntity.GetComponent<Health>();
            health.damage += CalculateAmount(damage, deviation, mult);
        }

        private void CreateBleeding(Entity pedEntity, float bleeding) {
            float bleedingDeviation = bleeding > 0 ? sharedData.mainConfig.WoundConfig.BleedingDeviation * bleeding : 0;
            float severity = bleeding + sharedData.random.NextFloat(-bleedingDeviation, bleedingDeviation);

            // var mult = _config.Data.WoundConfig.BleedingMultiplier;
            // var entity = _ecsWorld.CreateEntityWith(out BleedingComponent bleedingComponent);
            // bleedingComponent.Entity = pedEntity;
            // bleedingComponent.BleedSeverity = mult * bleedSeverity;
            // bleedingComponent.Name = name;
            // bleedingComponent.CanBeHealed = bleedSeverity <= mult * BleedingComponent.MaxSeverityForHeal;
            // woundedPed.BleedingCount++;
            // if (!bleedingComponent.CanBeHealed) return;
            //
            // if (woundedPed.MostDangerBleedingEntity == null ||
            //     !_ecsWorld.IsEntityExists(woundedPed.MostDangerBleedingEntity.Value))
            // {
            //     woundedPed.MostDangerBleedingEntity = entity;
            //     return;
            // }
            //
            // var oldBleeding = _ecsWorld.GetComponent<BleedingComponent>(woundedPed.MostDangerBleedingEntity.Value);
            // if (oldBleeding != null && oldBleeding.BleedSeverity >= bleedingComponent.BleedSeverity) return;
            //
            // woundedPed.MostDangerBleedingEntity = entity;
        }

        private void CreateCrit(Entity pedEntity, bool hasCrits) {
            // switch (crit) {
            //     case CritTypes.LEGS_DAMAGED:
            //         _ecsWorld.CreateEntityWith<LegsCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     case CritTypes.ARMS_DAMAGED:
            //         _ecsWorld.CreateEntityWith<ArmsCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     case CritTypes.NERVES_DAMAGED:
            //         _ecsWorld.CreateEntityWith<NervesCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     case CritTypes.GUTS_DAMAGED:
            //         _ecsWorld.CreateEntityWith<GutsCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     case CritTypes.STOMACH_DAMAGED:
            //         _ecsWorld.CreateEntityWith<StomachCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     case CritTypes.LUNGS_DAMAGED:
            //         _ecsWorld.CreateEntityWith<LungsCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     case CritTypes.HEART_DAMAGED:
            //         _ecsWorld.CreateEntityWith<HeartCriticalWoundEvent>().Entity = pedEntity;
            //         break;
            //     default: throw new ArgumentOutOfRangeException();
            // }
        }

        private void CreatePain(Entity pedEntity, float painAmount) {
            float pain = sharedData.mainConfig.WoundConfig.PainMultiplier * painAmount;

            // var painComponent = _ecsWorld.CreateEntityWith<IncreasePainEvent>();
            // painComponent.Entity = pedEntity;
            // painComponent.PainAmount = pain;
        }

        private void SendWoundInfo(in WoundData woundData) {
            if (!string.IsNullOrEmpty(woundData.AdditionalMessage)) {
                sharedData.notifier.warning.AddMessage(woundData.AdditionalMessage);
            }

            if (woundData.HasCrits
                || woundData.ArterySevered
                || woundData.BleedSeverity >= sharedData.mainConfig.WoundConfig.EmergencyBleedingLevel) {
                sharedData.notifier.emergency.AddMessage(woundData.Name);
            } else {
                sharedData.notifier.alert.AddMessage(woundData.Name);
            }

            if (woundData.ArterySevered) {
                sharedData.notifier.emergency.AddMessage(sharedData.localeConfig.SeveredArteryMessage);
            }
        }

        private float CalculateAmount(float baseAmount, float deviation, float mult) {
            float damageDeviation = baseAmount > 0 ? deviation * baseAmount : 0;
            baseAmount += sharedData.random.NextFloat(-damageDeviation, damageDeviation);
            baseAmount *= mult;
            return baseAmount;
        }
    }
}