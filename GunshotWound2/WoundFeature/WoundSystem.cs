namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;
    using GTA;
    using HealthFeature;
    using HitDetection;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using TraumaFeature;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class WoundSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly ArmorChecker armorChecker;

        private Filter damagedPeds;

        public EcsWorld World { get; set; }

        private WoundConfig WoundConfig => sharedData.mainConfig.woundConfig;

        public WoundSystem(SharedData sharedData) {
            this.sharedData = sharedData;
            armorChecker = new ArmorChecker(sharedData);
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>().With<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in damagedPeds) {
                ref PedHitData hitData = ref entity.GetComponent<PedHitData>();
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                ProcessHit(entity, ref hitData, ref convertedPed);
            }
        }

        void IDisposable.Dispose() { }

        private void ProcessHit(EcsEntity entity, ref PedHitData hitData, ref ConvertedPed convertedPed) {
            if (!hitData.weaponType.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Hit with no weapon");
#endif
                return;
            }

            if (hitData.weaponType.Key == nameof(WeaponConfig.Stun)) {
                CreateStunPain(entity, ref convertedPed);
                return;
            }

            if (!hitData.bodyPart.IsValid) {
                hitData.bodyPart = sharedData.mainConfig.bodyPartConfig.GetBodyPartByBone(Bone.SkelRoot);
                sharedData.logger.WriteWarning($"Hit has no bodyPart, {hitData.bodyPart.Key} will be used");
            }

            WoundConfig.Wound wound;
            if (armorChecker.TrySave(ref convertedPed, hitData, out WoundConfig.Wound armorWound)) {
                wound = armorWound;
            } else {
                string woundKey = sharedData.weightRandom.GetValueWithWeights(hitData.weaponType.Wounds);
#if DEBUG
                sharedData.logger.WriteInfo($"Selected wound {woundKey}");
#endif
                wound = sharedData.mainConfig.woundConfig.Wounds[woundKey];
            }

            Ped ped = convertedPed.thisPed;
            ped.Armor += hitData.armorDiff;
            ped.Health += hitData.healthDiff;

#if DEBUG
            if (!wound.IsValid) {
                sharedData.logger.WriteWarning("No wound created or wound is invalid!");
            } else {
                sharedData.logger.WriteInfo($"New wound {wound.ToString()} ");
            }
#endif

            ProcessWound(entity, hitData, wound, convertedPed);
        }

        private void CreateStunPain(EcsEntity pedEntity, ref ConvertedPed convertedPed) {
            if (!sharedData.mainConfig.weaponConfig.UseSpecialStunDamage) {
                return;
            }

            const float stunPainMult = 1.2f;
            float maxPain = convertedPed.isPlayer
                    ? sharedData.mainConfig.playerConfig.PainShockThreshold
                    : sharedData.mainConfig.pedsConfig.MaxPainShockThreshold;

            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            pain.diff += stunPainMult * maxPain - pain.amount;

            convertedPed.thisPed.PlayAmbientSpeech("PAIN_TAZER", SpeechModifier.InterruptShouted);
        }

        private void ProcessWound(EcsEntity entity, in PedHitData hitData, in WoundConfig.Wound wound, in ConvertedPed convertedPed) {
            if (!wound.IsValid) {
                return;
            }

            string woundName = sharedData.localeConfig.GetTranslation(wound.LocKey);
            ref Health health = ref entity.GetComponent<Health>();
            if (IsDeadlyWound(hitData, convertedPed.isPlayer)) {
#if DEBUG
                sharedData.logger.WriteInfo("It's deadly wound");
#endif
                health.InstantKill(woundName);
                return;
            }

            if (wound.Damage > 0f) {
                float mult = hitData.weaponType.DamageMult * WoundConfig.OverallDamageMult;
                float damageAmount = CalculateAmount(wound.Damage, WoundConfig.DamageDeviation, mult);
                damageAmount = Math.Max(damageAmount, 1f);
                health.DealDamage(damageAmount, woundName);
            }

            var finalBleed = 0f;
            if (wound.Bleed > 0f) {
                float mult = hitData.weaponType.BleedMult * WoundConfig.OverallBleedingMult;
                finalBleed = CalculateAmount(wound.Bleed, WoundConfig.BleedingDeviation, mult);
                entity.CreateBleeding(hitData.bodyPart, finalBleed, woundName, hitData.weaponType.ShortDesc, isTrauma: false);
            }

            var finalPain = 0f;
            if (wound.Pain > 0f) {
                float mult = hitData.weaponType.PainMult * WoundConfig.OverallPainMult;
                if (hitData.afterTakedown) {
                    mult *= sharedData.mainConfig.woundConfig.TakedownPainMult;
                }

                finalPain = CalculateAmount(wound.Pain, WoundConfig.PainDeviation, mult);
                entity.GetComponent<Pain>().diff += finalPain;
            }

            bool shouldRequestTrauma = ShouldRequestTrauma(wound, hitData);
            if (shouldRequestTrauma) {
                ref Traumas traumas = ref entity.AddOrGetComponent<Traumas>();
                traumas.requestBodyPart = hitData.bodyPart;
                traumas.forBluntDamage = wound.IsBlunt;
            }

            SendWoundInfo(convertedPed, health, hitData, woundName, finalBleed, finalPain, shouldRequestTrauma);
        }

        private bool ShouldRequestTrauma(in WoundConfig.Wound wound, in PedHitData hitData) {
            if (!wound.CanCauseTrauma) {
                return false;
            }

            if (!sharedData.random.IsTrueWithProbability(hitData.bodyPart.TraumaChance)) {
                return false;
            }

            if (hitData.bodyPart.IgnoreWeaponTraumaChance) {
                return true;
            }

            float weaponTraumaChance = hitData.weaponType.ChanceToCauseTrauma;
            return sharedData.random.IsTrueWithProbability(weaponTraumaChance);
        }

        private bool IsDeadlyWound(in PedHitData hitData, bool isPlayer) {
            return hitData.bodyPart.Bones.Contains((int)Bone.SkelHead)
                   && (isPlayer
                           ? sharedData.mainConfig.playerConfig.InstantDeathHeadshot
                           : sharedData.mainConfig.pedsConfig.InstantDeathHeadshot);
        }

        private void SendWoundInfo(in ConvertedPed convertedPed,
                                   in Health health,
                                   in PedHitData hitData,
                                   string woundName,
                                   float finalBleed,
                                   float finalPain,
                                   bool causeTrauma) {
            if (!convertedPed.isPlayer) {
                return;
            }

            const float criticalPain = 50f;
            Notifier.Entry notifier;
            if (causeTrauma
                || finalPain > criticalPain
                || finalBleed > health.CalculateDeadlyBleedingThreshold(convertedPed)) {
                notifier = sharedData.notifier.critical;
            } else {
                notifier = sharedData.notifier.wounds;
            }

            string bodyPart = sharedData.localeConfig.GetTranslation(hitData.bodyPart.LocKey);
            string traumaMessage = causeTrauma ? sharedData.localeConfig.WoundLeadsToTrauma : "";
            woundName = $"{woundName} ({bodyPart}) {traumaMessage}";

            Notifier.Color bleedingColor = health.GetBleedingColor(convertedPed, finalBleed);
            notifier.QueueMessage(woundName, bleedingColor);
        }

        private float CalculateAmount(float baseAmount, float deviation, float mult) {
            float damageDeviation = baseAmount > 0 ? deviation * baseAmount : 0;
            baseAmount += sharedData.random.NextFloat(-damageDeviation, damageDeviation);
            baseAmount *= mult;
            return baseAmount;
        }
    }
}