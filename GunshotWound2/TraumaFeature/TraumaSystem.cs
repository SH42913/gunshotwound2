namespace GunshotWound2.TraumaFeature {
    using Configs;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class TraumaSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly (Traumas.Effects, BaseTraumaEffect)[] effects;

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
            };
        }

        public void OnAwake() {
            pedsWithTraumas = World.Filter.With<ConvertedPed>().With<Traumas>();
            traumasStash = World.GetStash<Traumas>();
            pedStash = World.GetStash<ConvertedPed>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in pedsWithTraumas) {
                ref Traumas traumas = ref traumasStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (totallyHealedStash.Has(entity)) {
                    CancelAllTraumaEffects(entity, ref traumas, ref convertedPed);
                } else if (traumas.requestBodyPart.IsValid) {
                    ProcessRequest(entity, ref traumas, ref convertedPed);
                    traumas.requestBodyPart = default;
                } else {
                    IterateTraumaEffects(entity, ref traumas, ref convertedPed);
                }
            }
        }

        private void ProcessRequest(Entity entity, ref Traumas traumas, ref ConvertedPed convertedPed) {
            var possibleTraumas = traumas.forBluntDamage
                    ? traumas.requestBodyPart.BluntTraumas
                    : traumas.requestBodyPart.PenetratingTraumas;

            string traumaKey = sharedData.weightRandom.GetValueWithWeights(possibleTraumas);
#if DEBUG
            sharedData.logger.WriteInfo($"Selected crit {traumaKey} for {traumas.requestBodyPart.Key} at {convertedPed.name}");
#endif
            TraumaConfig.Trauma trauma = sharedData.mainConfig.traumaConfig.Traumas[traumaKey];
            string traumaName = sharedData.localeConfig.GetTranslation(trauma.LocKey);
            string reason = sharedData.localeConfig.TraumaType;
            entity.GetComponent<Pain>().diff += trauma.Pain;
            entity.GetComponent<Health>().DealDamage(trauma.Damage, traumaName);
            entity.CreateBleeding(traumas.requestBodyPart, trauma.Bleed, traumaName, reason, isTrauma: true);

            if (trauma.Effect != Traumas.Effects.Spine) {
                ApplyTraumaEffect(entity, ref traumas, ref convertedPed, trauma.Effect, trauma.EffectMessage);
            } else {
                bool realSpineTrauma = convertedPed.isPlayer
                        ? sharedData.mainConfig.playerConfig.RealisticSpineDamage
                        : sharedData.mainConfig.pedsConfig.RealisticSpineDamage;

                if (realSpineTrauma) {
                    ApplyTraumaEffect(entity, ref traumas, ref convertedPed, Traumas.Effects.Spine, trauma.EffectMessage);
                } else {
                    ApplyTraumaEffect(entity, ref traumas, ref convertedPed, Traumas.Effects.Arms, trauma.EffectMessage);
                    ApplyTraumaEffect(entity, ref traumas, ref convertedPed, Traumas.Effects.Legs, trauma.EffectMessage);
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
                if (newEffect != type || traumas.HasActive(type)) {
                    continue;
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
                traumas.requestBodyPart = default;
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