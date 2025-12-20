namespace GunshotWound2.WoundFeature {
    using System;
    using Configs;
    using GTA;
    using GTA.Math;
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

            Ped ped = convertedPed.thisPed;
            ped.Armor += hitData.armorDiff;
            ped.Health += hitData.healthDiff;

            if (!hitData.bodyPart.IsValid) {
                hitData.bodyPart = sharedData.mainConfig.bodyPartConfig.GetBodyPartByBone(Bone.SkelRoot);
                sharedData.logger.WriteWarning($"Hit has no bodyPart, {hitData.bodyPart.Key} will be used");
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Processing {hitData.hits} hits");
#endif
            bool isMultPelletWeapon = hitData.weaponType.Pellets > 1;
            if (hitData.hits == 1) {
                ProcessOneHit(entity, hitData, ref convertedPed);
            } else if (isMultPelletWeapon && hitData.hits > 0.5f * hitData.weaponType.Pellets) {
                ProcessOneHit(entity, hitData, ref convertedPed);
            } else {
                ProcessMultiHit(entity, hitData, ref convertedPed);
            }
        }

        private void ProcessMultiHit(EcsEntity entity, PedHitData hitData, ref ConvertedPed convertedPed) {
            int savedHits = hitData.hits;
            hitData.hits = 1;

            Random random = sharedData.random;
            BodyPartConfig.BodyPart[] bodyParts = sharedData.mainConfig.bodyPartConfig.BodyParts;
            for (int i = 0; i < savedHits; i++) {
                ProcessOneHit(entity, hitData, ref convertedPed);
                hitData.bodyPart = random.Next(bodyParts);
            }
        }

        private void ProcessOneHit(EcsEntity entity, in PedHitData hitData, ref ConvertedPed convertedPed) {
            WoundConfig.Wound wound;
            if (armorChecker.TrySave(ref convertedPed, hitData, out WoundConfig.Wound armorWound)) {
                wound = armorWound;
            } else if (hitData.afterTakedown && !string.IsNullOrEmpty(hitData.weaponType.TakedownWound)) {
                string takedownWound = hitData.weaponType.TakedownWound;
#if DEBUG
                sharedData.logger.WriteInfo($"Applying takedown wound {takedownWound}");
#endif
                wound = sharedData.mainConfig.woundConfig.Wounds[takedownWound];
            } else if (!string.IsNullOrEmpty(hitData.weaponType.TangentialWound) && CheckIsTangentialHit(hitData)) {
                string tangentialWound = hitData.weaponType.TangentialWound;
#if DEBUG
                sharedData.logger.WriteInfo($"Applying tangential wound {tangentialWound}");
#endif
                wound = sharedData.mainConfig.woundConfig.Wounds[tangentialWound];
            } else {
                string woundKey = sharedData.weightRandom.GetValueWithWeights(hitData.weaponType.Wounds);
#if DEBUG
                sharedData.logger.WriteInfo($"Selected wound {woundKey}");
#endif
                wound = sharedData.mainConfig.woundConfig.Wounds[woundKey];
            }

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

            ref Pain pain = ref pedEntity.GetComponent<Pain>();
            pain.diff += sharedData.mainConfig.weaponConfig.StunPainPercent * pain.max - pain.amount;
            pain.dontDelayDiff = true;
            convertedPed.thisPed.PlayAmbientSpeech("PAIN_TAZER", SpeechModifier.ForceShouted);
        }

        private void ProcessWound(EcsEntity targetEntity,
                                  in PedHitData hitData,
                                  in WoundConfig.Wound wound,
                                  in ConvertedPed convertedPed) {
            if (!wound.IsValid) {
                return;
            }

            string woundName = sharedData.localeConfig.GetTranslation(wound.LocKey);
            ref Health health = ref targetEntity.GetComponent<Health>();
            if (IsDeadlyWound(hitData, convertedPed.isPlayer)) {
#if DEBUG
                sharedData.logger.WriteInfo("It's deadly wound");
#endif
                health.InstantKill(woundName);
                return;
            }

            DBPContainer dbp = wound.DBP.Deviate(sharedData.random, sharedData.mainConfig.woundConfig.GlobalDeviations);
            dbp *= WoundConfig.GlobalMultipliers * hitData.weaponType.DBPMults * hitData.bodyPart.DBPMults;

            string reason = hitData.weaponType.ShortDesc;
            if (hitData.hits > 1) {
                dbp *= hitData.hits;
                reason += $" x{hitData.hits.ToString()}";
            }

            if (hitData.afterTakedown) {
                dbp *= WoundConfig.TakedownMults;
            }

            if (dbp.damage > 0f) {
                health.DealDamage(dbp.damage, woundName);
            }

            EcsEntity woundBleeding = null;
            if (dbp.bleed > 0f) {
                woundBleeding = targetEntity.CreateCommonBleeding(hitData.bodyPart, dbp.bleed, woundName, reason, wound.IsBlunt);
                woundBleeding.SetComponent(hitData);
                woundBleeding.SetComponent(new WoundData {
                    totalBleed = dbp.bleed,
                    totalPain = dbp.pain,
                });
            }

            if (dbp.pain > 0f) {
                targetEntity.GetComponent<Pain>().diff += dbp.pain;
            }

            bool shouldCreateTrauma = ShouldCreateTrauma(wound, hitData);
            if (shouldCreateTrauma) {
                World.CreateEntity().SetComponent(new TraumaRequest {
                    target = targetEntity,
                    targetBodyPart = hitData.bodyPart,
                    parentBleeding = woundBleeding,
                });
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Final wound DBP:{dbp.ToString()} trauma:{shouldCreateTrauma} by {reason}");
#endif

            SendWoundInfo(convertedPed, health, hitData, woundName, dbp.bleed, dbp.pain, shouldCreateTrauma);
        }

        private bool ShouldCreateTrauma(in WoundConfig.Wound wound, in PedHitData hitData) {
            if (!wound.CanCauseTrauma) {
                return false;
            }

            Random random = sharedData.random;
            float bodyPartTraumaChance = hitData.bodyPart.TraumaChance;
            if (!wound.IsBlunt && hitData.bodyPart.IgnoreWeaponTraumaChanceForPenetratingWounds) {
                return random.IsTrueWithProbability(bodyPartTraumaChance);
            }

            float oneHitChance = bodyPartTraumaChance * hitData.weaponType.ChanceToCauseTrauma;
            if (hitData.hits == 1) {
                return random.IsTrueWithProbability(oneHitChance);
            }

            double totalChance = 1f - Math.Pow(1f - oneHitChance, hitData.hits);
            return random.IsTrueWithProbability(totalChance);
        }

        private bool IsDeadlyWound(in PedHitData hitData, bool isPlayer) {
            return hitData.bodyPart.IsHead()
                   && (isPlayer
                           ? sharedData.mainConfig.playerConfig.InstantDeathHeadshot
                           : sharedData.mainConfig.pedsConfig.InstantDeathHeadshot);
        }

        private bool CheckIsTangentialHit(in PedHitData hitData) {
            if (!hitData.fullHitData) {
                return false;
            }

            float impactDot = Vector3.Dot(hitData.shotDir, hitData.hitNorm);
            float threshold = sharedData.mainConfig.weaponConfig.TangentialWoundThreshold;
            return Math.Abs(impactDot) < threshold;
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
    }
}