namespace GunshotWound2.WoundFeature {
    using System;
    using System.Collections.Generic;
    using Configs;
    using CritsFeature;
    using HealthFeature;
    using HitDetection;
    using PainFeature;
    using PedsFeature;
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
#if DEBUG
                    sharedData.logger.WriteWarning("Wound with no weapon");
#endif
                    continue;
                }

                if (weaponDamages.TryGetValue(hitData.weaponType, out BaseWeaponDamage weaponDamage)) {
                    ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
                    convertedPed.thisPed.Armor += hitData.armorDiff;
                    convertedPed.thisPed.Health += hitData.healthDiff;

                    WoundData? wound = weaponDamage.ProcessHit(ref convertedPed, ref hitData);
#if DEBUG
                    sharedData.logger.WriteInfo(wound.HasValue ? $"New wound {wound.Value.ToString()} " : "No wound created");
#endif
                    ProcessWound(pedEntity, ref hitData, ref wound);
                    SendWoundInfo(pedEntity, convertedPed, hitData, wound);
                } else {
                    sharedData.logger.WriteWarning($"Doesn't support {hitData.weaponType}");
                }
            }
        }

        private void ProcessWound(Entity pedEntity, ref PedHitData hitData, ref WoundData? wound) {
            if (!wound.HasValue) {
                return;
            }

            WoundData woundData = wound.Value;
            CreateDamage(pedEntity, woundData.Damage, woundData.Name);
            CreateBleeding(pedEntity, woundData.BleedSeverity, woundData.InternalBleeding, woundData.Name);
            CreatePain(pedEntity, woundData.Pain);
            CreateCrit(pedEntity, woundData.HasCrits, hitData.bodyPart);

            if (woundData.ArterySevered) {
                CreateBleeding(pedEntity, WoundConfig.MAX_SEVERITY_FOR_BANDAGE, isInternal: true,
                               sharedData.localeConfig.SeveredArtery);
            }
        }

        private void CreateDamage(Entity pedEntity, float damage, string woundName) {
            if (damage <= 0f) {
                return;
            }

            float deviation = sharedData.mainConfig.WoundConfig.DamageDeviation;
            float mult = sharedData.mainConfig.WoundConfig.DamageMultiplier;

            float damageAmount = CalculateAmount(damage, deviation, mult);
            damageAmount = Math.Max(damageAmount, 1f);
            pedEntity.GetComponent<Health>().DealDamage(damageAmount, woundName);
        }

        private void CreateBleeding(Entity pedEntity, float severity, bool isInternal, string name) {
            if (severity <= 0f) {
                return;
            }

            float deviation = sharedData.mainConfig.WoundConfig.BleedingDeviation;
            float mult = sharedData.mainConfig.WoundConfig.BleedingMultiplier;
            severity = CalculateAmount(severity, deviation, mult);
            pedEntity.CreateBleeding(severity, name, isInternal);
        }

        private void CreatePain(Entity pedEntity, float painAmount) {
            float deviation = sharedData.mainConfig.WoundConfig.PainDeviation;
            float mult = sharedData.mainConfig.WoundConfig.PainMultiplier;

            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            pain.diff += CalculateAmount(painAmount, deviation, mult);
        }

        private static void CreateCrit(Entity pedEntity, bool hasCrits, PedHitData.BodyParts hitBodyPart) {
            if (hasCrits) {
                pedEntity.AddOrGetComponent<Crits>().requestBodyPart = hitBodyPart;
            }
        }

        private void SendWoundInfo(Entity pedEntity, in ConvertedPed convertedPed, in PedHitData hitData, in WoundData? wound) {
            if (!convertedPed.isPlayer) {
                return;
            }

            sharedData.notifier.critical.QueueMessage(hitData.armorMessage, Notifier.Color.YELLOW);
            if (!wound.HasValue) {
                return;
            }

            Notifier.Entry notifier;
            WoundData woundData = wound.Value;
            float deadlyBleedingThreshold = pedEntity.GetComponent<Health>().CalculateDeadlyBleedingThreshold(convertedPed);
            if (woundData.HasCrits
                || woundData.ArterySevered
                || woundData.BleedSeverity >= deadlyBleedingThreshold) {
                notifier = sharedData.notifier.critical;
            } else {
                notifier = sharedData.notifier.wounds;
            }

            notifier.QueueMessage(woundData.Name);

            if (woundData.ArterySevered) {
                notifier.QueueMessage(sharedData.localeConfig.SeveredArteryMessage);
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