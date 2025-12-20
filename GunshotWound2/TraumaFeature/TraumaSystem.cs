namespace GunshotWound2.TraumaFeature {
    using System;
    using System.Collections.Generic;
    using Configs;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using WoundFeature;

    public sealed class TraumaSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly (Traumas.Effects, BaseTraumaEffect)[] effects;

        private Filter traumaRequests;
        private Filter pedsWithTraumas;
        private Stash<Traumas> traumasStash;
        private Stash<ConvertedPed> pedStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;

        public World World { get; set; }

        public TraumaSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            effects = new (Traumas.Effects, BaseTraumaEffect)[] {
                (Traumas.Effects.Arms, new ArmsTraumaEffect(this.sharedData)),
                (Traumas.Effects.Legs, new LegsTraumaEffect(this.sharedData)),
                (Traumas.Effects.Heart, new HeartTraumaEffect(this.sharedData)),
                (Traumas.Effects.Lungs, new LungsTraumaEffect(this.sharedData)),
                (Traumas.Effects.Abdomen, new AbdomenTraumaEffect(this.sharedData)),
                (Traumas.Effects.Spine, new SpineTraumaEffect(this.sharedData)),
                (Traumas.Effects.Head, new HeadTraumaEffect(this.sharedData)),
            };
        }

        public void OnAwake() {
            traumaRequests = World.Filter.With<TraumaRequest>();
            pedsWithTraumas = World.Filter.With<ConvertedPed>().With<Traumas>();

            traumasStash = World.GetStash<Traumas>();
            pedStash = World.GetStash<ConvertedPed>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            ProcessRequests();
            UpdateActiveTraumas();
        }

        private void ProcessRequests() {
            foreach (Entity entity in traumaRequests) {
                ref TraumaRequest request = ref entity.GetComponent<TraumaRequest>();
                if (request.target.IsNullOrDisposed()) {
                    sharedData.logger.WriteWarning("Can't find target for trauma");
                    World.RemoveEntity(entity);
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(request.target, out bool isConverted);
                if (!isConverted) {
                    sharedData.logger.WriteWarning("Not valid target for trauma");
                    World.RemoveEntity(entity);
                    continue;
                }

                ProcessRequest(entity, ref request, ref convertedPed);
                entity.RemoveComponent<TraumaRequest>();
            }
        }

        private void UpdateActiveTraumas() {
            foreach (Entity entity in pedsWithTraumas) {
                ref Traumas traumas = ref traumasStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (totallyHealedStash.Has(entity)) {
                    CancelAllTraumaEffects(entity, ref traumas, ref convertedPed);
                } else {
                    IterateTraumaEffects(entity, ref traumas, ref convertedPed);
                }
            }
        }

        private void ProcessRequest(Entity traumaEntity, ref TraumaRequest request, ref ConvertedPed convertedPed) {
            BodyPartConfig.BodyPart bodyPart = request.targetBodyPart;

            var bluntDamage = true;
            Entity parentEntity = request.parentBleeding;
            bool parentEntityExists = !parentEntity.IsNullOrDisposed();
            if (parentEntityExists) {
                ref Bleeding bleeding = ref parentEntity.GetComponent<Bleeding>(out bool hasBleeding);
                if (hasBleeding) {
                    bluntDamage = bleeding.bluntDamageReason;
                }
            }

            (string key, int weight)[] possibleTraumas = bluntDamage
                    ? bodyPart.BluntTraumas
                    : bodyPart.PenetratingTraumas;

            string traumaKey = sharedData.weightRandom.GetValueWithWeights(possibleTraumas);
#if DEBUG
            sharedData.logger.WriteInfo($"Selected crit {traumaKey} for {bodyPart.Key} at {convertedPed.name}");
#endif
            TraumaConfig.Trauma trauma = sharedData.mainConfig.traumaConfig.Traumas[traumaKey];
            DBPContainer dbp = trauma.DBP * sharedData.mainConfig.woundConfig.GlobalMultipliers * bodyPart.DBPMults;
#if DEBUG
            sharedData.logger.WriteInfo($"Final trauma DBP:{dbp.ToString()}");
#endif

            Entity targetEntity = request.target;
            string traumaName = sharedData.localeConfig.GetTranslation(trauma.LocKey);
            targetEntity.GetComponent<Health>().DealDamage(dbp.damage, traumaName);
            targetEntity.GetComponent<Pain>().diff += dbp.pain;

            string reason = sharedData.localeConfig.TraumaType;
            float severity = Math.Max(0.01f, dbp.bleed);
            targetEntity.CreateTraumaBleeding(bodyPart, severity, traumaName, reason, traumaEntity);

            if (parentEntityExists) {
                ref WoundData woundData = ref parentEntity.GetComponent<WoundData>(out bool hasWoundData);
                if (hasWoundData) {
                    woundData.totalBleed += severity;
                    woundData.totalPain += dbp.pain;
                }
            }

            if (trauma.CanGeneratePain) {
                traumaEntity.SetComponent(new PainGenerator {
                    target = targetEntity,
                    moveRate = trauma.PainRateWhenMoving,
                    runRate = trauma.PainRateWhenRunning,
                    aimRate = trauma.PainRateWhenAiming,
                });
            }

            ref Traumas activeTraumas = ref traumasStash.AddOrGet(targetEntity);
            activeTraumas.traumas ??= new HashSet<Entity>(4);
            activeTraumas.traumas.Add(traumaEntity);

            if (trauma.Effect != Traumas.Effects.Spine) {
                ApplyTraumaEffect(targetEntity, ref activeTraumas, ref convertedPed, trauma.Effect, trauma.EffectMessage);
            } else {
                bool realSpineTrauma = convertedPed.isPlayer
                        ? sharedData.mainConfig.playerConfig.RealisticSpineDamage
                        : sharedData.mainConfig.pedsConfig.RealisticSpineDamage;

                if (realSpineTrauma) {
                    ApplyTraumaEffect(targetEntity, ref activeTraumas, ref convertedPed, Traumas.Effects.Spine, trauma.EffectMessage);
                } else {
                    ApplyTraumaEffect(targetEntity, ref activeTraumas, ref convertedPed, Traumas.Effects.Arms, trauma.EffectMessage);
                    ApplyTraumaEffect(targetEntity, ref activeTraumas, ref convertedPed, Traumas.Effects.Legs, trauma.EffectMessage);
                }
            }
        }

        public void Dispose() {
            foreach (Entity entity in pedsWithTraumas) {
                ref Traumas traumas = ref traumasStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                CancelAllTraumaEffects(entity, ref traumas, ref convertedPed);
            }
        }

        private void ApplyTraumaEffect(Entity entity,
                                       ref Traumas traumas,
                                       ref ConvertedPed convertedPed,
                                       Traumas.Effects newEffect,
                                       bool showEffectMessage) {
            foreach ((Traumas.Effects type, BaseTraumaEffect effect) in effects) {
                if (newEffect != type) {
                    continue;
                }

                if (traumas.HasActive(type)) {
                    effect.Repeat(entity, ref convertedPed);
                    break;
                }

#if DEBUG
                sharedData.logger.WriteInfo($"Apply trauma effect for {convertedPed.name} - {type}");
#endif

                if (showEffectMessage && !convertedPed.hasSpineDamage) {
                    if (convertedPed.isPlayer) {
                        sharedData.notifier.critical.QueueMessage(effect.PlayerMessage, Notifier.Color.YELLOW);
                    } else {
                        string message = convertedPed.isMale ? effect.ManMessage : effect.WomanMessage;
                        sharedData.notifier.peds.QueueMessage(message);
                    }
                }

                effect.Apply(entity, ref convertedPed);
                traumas.activeEffects |= type;
                break;
            }
        }

        private void IterateTraumaEffects(Entity entity, ref Traumas traumas, ref ConvertedPed convertedPed) {
            foreach ((Traumas.Effects type, BaseTraumaEffect action) in effects) {
                if (traumas.HasActive(type)) {
                    action.EveryFrame(entity, ref convertedPed);
                }
            }
        }

        private void CancelAllTraumaEffects(Entity entity, ref Traumas traumas, ref ConvertedPed convertedPed) {
            foreach ((Traumas.Effects type, BaseTraumaEffect action) in effects) {
                if (traumas.HasActive(type)) {
                    action.Cancel(entity, ref convertedPed);
                }
            }

            entity.RemoveComponent<Traumas>();
        }
    }
}