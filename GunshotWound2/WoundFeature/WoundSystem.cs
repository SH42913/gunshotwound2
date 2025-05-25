namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;
    using CritsFeature;
    using GTA;
    using HealthFeature;
    using HitDetection;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class WoundSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly ArmorChecker armorChecker;

        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        private WoundConfig WoundConfig => sharedData.mainConfig.woundConfig;

        public WoundSystem(SharedData sharedData) {
            this.sharedData = sharedData;
            armorChecker = new ArmorChecker(sharedData);
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>().With<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity pedEntity in damagedPeds) {
                ref PedHitData hitData = ref pedEntity.GetComponent<PedHitData>();
                ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
                ProcessHit(pedEntity, ref hitData, ref convertedPed);
            }
        }

        void IDisposable.Dispose() { }

        private void ProcessHit(Scellecs.Morpeh.Entity pedEntity, ref PedHitData hitData, ref ConvertedPed convertedPed) {
            if (!hitData.weaponType.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Hit with no weapon");
#endif
                return;
            }

            if (hitData.weaponType.Key == nameof(WeaponConfig.Stun)) {
                CreateStunPain(pedEntity, ref convertedPed);
                return;
            }

            WoundData? wound;
            Ped ped = convertedPed.thisPed;
            if (!hitData.bodyPart.IsValid) {
                sharedData.logger.WriteWarning("Hit has no bodyPart, will be used default wound");
                WoundConfig.Wound woundTemplate = sharedData.mainConfig.woundConfig.Wounds["GrazeDefault"];
                wound = new WoundData(sharedData.localeConfig, sharedData.random, woundTemplate, hitData);
            } else if (armorChecker.TrySave(ref convertedPed, hitData, hitData.weaponType, out WoundData? armorWound)) {
                wound = armorWound;
            } else {
                WoundConfig.Wound woundTemplate = GetRandomWoundTemplate(hitData.weaponType);
                wound = new WoundData(sharedData.localeConfig, sharedData.random, woundTemplate, hitData);
            }

            ped.Armor += hitData.armorDiff;
            ped.Health += hitData.healthDiff;

#if DEBUG
            if (wound.HasValue) {
                sharedData.logger.WriteInfo($"New wound {wound.Value.ToString()} ");
            } else {
                sharedData.logger.WriteWarning("No wound created!");
            }
#endif
            ProcessWound(pedEntity, ref hitData, ref wound, convertedPed.isPlayer);
            SendWoundInfo(pedEntity, convertedPed, wound);
        }

        private void CreateStunPain(Scellecs.Morpeh.Entity pedEntity, ref ConvertedPed convertedPed) {
            if (!sharedData.mainConfig.weaponConfig.UseSpecialStunDamage) {
                return;
            }

            const float stunPainMult = 1.2f;
            float maxPain = convertedPed.isPlayer
                    ? sharedData.mainConfig.playerConfig.PainShockThreshold
                    : sharedData.mainConfig.pedsConfig.MaxPainShockThreshold;

            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            pain.diff += (stunPainMult * maxPain - pain.amount);

            convertedPed.thisPed.PlayAmbientSpeech("PAIN_TAZER", SpeechModifier.InterruptShouted);
        }

        private WoundConfig.Wound GetRandomWoundTemplate(in WeaponConfig.Weapon weapon) {
            string woundKey = sharedData.weightRandom.GetValueWithWeights(weapon.Wounds);
            WoundConfig.Wound woundTemplate = sharedData.mainConfig.woundConfig.Wounds[woundKey];
            return woundTemplate;
        }

        private void ProcessWound(Scellecs.Morpeh.Entity pedEntity, ref PedHitData hitData, ref WoundData? wound, bool isPlayer) {
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

            if (hitData.afterTakedown) {
                woundData.Pain *= sharedData.mainConfig.pedsConfig.TakedownPainMult;
            }

            CreateDamage(pedEntity, woundData.Damage, woundData.Name);
            CreateBleeding(pedEntity, hitData.bodyPart, woundData.BleedSeverity, woundData.InternalBleeding, woundData.Name);
            CreatePain(pedEntity, woundData.Pain);
            CreateCrit(pedEntity, woundData.HasCrits, hitData.bodyPart);

            if (woundData.ArterySevered) {
                CreateArteryBleeding(pedEntity, hitData.bodyPart);
            }
        }

        private bool IsDeadlyWound(in PedHitData hitData, bool isPlayer) {
            return hitData.bodyPart.Bones.Contains((int)Bone.SkelHead)
                   && (isPlayer
                           ? sharedData.mainConfig.playerConfig.InstantDeathHeadshot
                           : sharedData.mainConfig.pedsConfig.InstantDeathHeadshot);
        }

        private static void InstantKill(Scellecs.Morpeh.Entity pedEntity, string woundName) {
            pedEntity.GetComponent<Health>().InstantKill(woundName);
        }

        private void CreateDamage(Scellecs.Morpeh.Entity pedEntity, float damage, string woundName) {
            if (damage <= 0f) {
                return;
            }

            float deviation = WoundConfig.DamageDeviation;
            float mult = WoundConfig.OverallDamageMult;

            float damageAmount = CalculateAmount(damage, deviation, mult);
            damageAmount = Math.Max(damageAmount, 1f);
            pedEntity.GetComponent<Health>().DealDamage(damageAmount, woundName);
        }

        private void CreateBleeding(Scellecs.Morpeh.Entity pedEntity,
                                    in BodyPartConfig.BodyPart bodyPart,
                                    float severity,
                                    bool isInternal,
                                    string name) {
            if (severity <= 0f) {
                return;
            }

            float deviation = WoundConfig.BleedingDeviation;
            float mult = WoundConfig.OverallBleedingMult;
            severity = CalculateAmount(severity, deviation, mult);
            pedEntity.CreateBleeding(bodyPart, severity, name, isInternal);
        }

        private void CreateArteryBleeding(Scellecs.Morpeh.Entity pedEntity, BodyPartConfig.BodyPart bodyPart) {
            string name = sharedData.localeConfig.SeveredArtery;
            CreateBleeding(pedEntity, bodyPart, WoundConfig.MAX_SEVERITY_FOR_BANDAGE, isInternal: true, name);
        }

        private void CreatePain(Scellecs.Morpeh.Entity pedEntity, float painAmount) {
            float deviation = WoundConfig.PainDeviation;
            float mult = WoundConfig.OverallPainMult;

            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            pain.diff += CalculateAmount(painAmount, deviation, mult);
        }

        private static void CreateCrit(Scellecs.Morpeh.Entity pedEntity, bool hasCrits, BodyPartConfig.BodyPart hitBodyPart) {
            if (hasCrits) {
                pedEntity.AddOrGetComponent<Crits>().requestBodyPart = hitBodyPart;
            }
        }

        private void SendWoundInfo(Scellecs.Morpeh.Entity pedEntity,
                                   in ConvertedPed convertedPed,
                                   in WoundData? wound) {
            if (!convertedPed.isPlayer || !wound.HasValue) {
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