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

    public sealed class WoundSystem : ILateSystem {
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

                ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
                if (hitData.weaponType == PedHitData.WeaponTypes.Stun) {
                    CreateStunPain(pedEntity, ref convertedPed);
                    continue;
                }

                if (weaponDamages.TryGetValue(hitData.weaponType, out BaseWeaponDamage weaponDamage)) {
                    convertedPed.thisPed.Armor += hitData.armorDiff;
                    convertedPed.thisPed.Health += hitData.healthDiff;
                    convertedPed.lastDamagedBone = hitData.damagedBone;

                    WoundData? wound = weaponDamage.ProcessHit(ref convertedPed, ref hitData);
#if DEBUG
                    sharedData.logger.WriteInfo(wound.HasValue ? $"New wound {wound.Value.ToString()} " : "No wound created");
#endif
                    ProcessWound(pedEntity, ref hitData, ref wound, convertedPed.isPlayer);
                    SendWoundInfo(pedEntity, convertedPed, hitData, wound);
                } else {
                    sharedData.logger.WriteWarning($"Doesn't support {hitData.weaponType}");
                }
            }
        }

        private void CreateStunPain(Entity pedEntity, ref ConvertedPed convertedPed) {
            const float stunPainMult = 1.2f;
            float maxPain = convertedPed.isPlayer
                    ? sharedData.mainConfig.PlayerConfig.MaximalPain
                    : sharedData.mainConfig.NpcConfig.UpperMaximalPain;

            CreatePain(pedEntity, stunPainMult * maxPain);
            convertedPed.thisPed.PlayAmbientSpeech("PAIN_TAZER", GTA.SpeechModifier.InterruptShouted);
        }

        private void ProcessWound(Entity pedEntity, ref PedHitData hitData, ref WoundData? wound, bool isPlayer) {
            if (!wound.HasValue) {
                return;
            }

            WoundData woundData = wound.Value;
            if (IsDeadlyWound(hitData, isPlayer)) {
#if DEBUG
                sharedData.logger.WriteInfo("It's deadly wound");
#endif
                InstantKill(pedEntity, woundData.Name);
                return;
            }

            CreateDamage(pedEntity, woundData.Damage, woundData.Name);
            CreateBleeding(pedEntity, woundData.BleedSeverity, woundData.InternalBleeding, woundData.Name);
            CreatePain(pedEntity, woundData.Pain);
            CreateCrit(pedEntity, woundData.HasCrits, hitData.bodyPart);

            if (woundData.ArterySevered) {
                CreateBleeding(pedEntity, WoundConfig.MAX_SEVERITY_FOR_BANDAGE, isInternal: true,
                               sharedData.localeConfig.SeveredArtery);
            }
        }

        private bool IsDeadlyWound(in PedHitData hitData, bool isPlayer) {
            return hitData.bodyPart == PedHitData.BodyParts.Head
                   && (isPlayer
                           ? sharedData.mainConfig.PlayerConfig.InstantDeathHeadshot
                           : sharedData.mainConfig.NpcConfig.InstantDeathHeadshot);
        }

        private static void InstantKill(Entity pedEntity, string woundName) {
            pedEntity.GetComponent<Health>().InstantKill(woundName);
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
            ref Health health = ref pedEntity.GetComponent<Health>();
            if (woundData.HasCrits
                || woundData.ArterySevered
                || woundData.BleedSeverity > health.CalculateDeadlyBleedingThreshold(convertedPed)) {
                notifier = sharedData.notifier.critical;
            } else {
                notifier = sharedData.notifier.wounds;
            }

            Notifier.Color bleedingColor = health.GetBleedingColor(convertedPed, woundData.BleedSeverity);
            notifier.QueueMessage(woundData.Name, bleedingColor);

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