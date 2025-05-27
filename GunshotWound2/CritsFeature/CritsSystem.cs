namespace GunshotWound2.CritsFeature {
    using Configs;
    using HealthFeature;
    using HitDetection;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class CritsSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly (Crits.Effects, BaseCrit)[] critEffects;

        private Filter pedsWithCrits;
        private Stash<Crits> critsStash;
        private Stash<ConvertedPed> pedStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;

        public World World { get; set; }

        public CritsSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            critEffects = new (Crits.Effects, BaseCrit)[] {
                (Crits.Effects.ArmsCrit, new ArmsCrit(this.sharedData)),
                (Crits.Effects.LegsCrit, new LegsCrit(this.sharedData)),
                (Crits.Effects.HeartCrit, new HeartCrit(this.sharedData)),
                (Crits.Effects.LungsCrit, new LungsCrit(this.sharedData)),
                (Crits.Effects.GutsCrit, new GutsCrit(this.sharedData)),
                (Crits.Effects.StomachCrit, new StomachCrit(this.sharedData)),
                (Crits.Effects.SpineCrit, new SpineCrit(this.sharedData)),
            };
        }

        public void OnAwake() {
            pedsWithCrits = World.Filter.With<ConvertedPed>().With<Crits>();
            critsStash = World.GetStash<Crits>();
            pedStash = World.GetStash<ConvertedPed>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in pedsWithCrits) {
                ref Crits crits = ref critsStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (totallyHealedStash.Has(entity)) {
                    CancelAllCritEffects(entity, ref crits, ref convertedPed);
                } else if (crits.requestBodyPart.IsValid) {
                    ProcessRequest(entity, ref crits, ref convertedPed);
                    crits.requestBodyPart = default;
                } else {
                    IterateCritEffects(entity, ref crits, ref convertedPed);
                }
            }
        }

        private void ProcessRequest(Entity entity, ref Crits crits, ref ConvertedPed convertedPed) {
            if (!sharedData.random.IsTrueWithProbability(crits.requestBodyPart.CritChance)) {
#if DEBUG
                sharedData.logger.WriteInfo($"Ignore crit for {crits.requestBodyPart.Key} at {convertedPed.name}");
#endif
                return;
            }

            string critKey = sharedData.weightRandom.GetValueWithWeights(crits.requestBodyPart.Crits);
            CritsConfig.Crit crit = sharedData.mainConfig.critsConfig.GetCritByKey(critKey);
#if DEBUG
            sharedData.logger.WriteInfo($"Selected crit {crit.Key} for {crits.requestBodyPart.Key} at {convertedPed.name}");
#endif

            string critName = sharedData.localeConfig.GetTranslation(crit.LocKey);
            string reason = sharedData.localeConfig.TraumaType;
            entity.GetComponent<Pain>().diff += crit.Pain;
            entity.GetComponent<Health>().DealDamage(crit.Damage, critName);
            entity.CreateBleeding(crits.requestBodyPart, crit.Bleed, critName, reason, isInternal: true);

            if (crit.Effect != Crits.Effects.SpineCrit) {
                ApplyCritEffect(entity, ref crits, ref convertedPed, crit.Effect, crit.Message);
            } else {
                bool realSpineCrit = convertedPed.isPlayer
                        ? sharedData.mainConfig.playerConfig.RealisticSpineDamage
                        : sharedData.mainConfig.pedsConfig.RealisticSpineDamage;

                if (realSpineCrit) {
                    ApplyCritEffect(entity, ref crits, ref convertedPed, Crits.Effects.SpineCrit, crit.Message);
                } else {
                    ApplyCritEffect(entity, ref crits, ref convertedPed, Crits.Effects.ArmsCrit, crit.Message);
                    ApplyCritEffect(entity, ref crits, ref convertedPed, Crits.Effects.LegsCrit, crit.Message);
                }
            }
        }

        public void Dispose() {
            foreach (Entity entity in pedsWithCrits) {
                ref Crits crits = ref critsStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                CancelAllCritEffects(entity, ref crits, ref convertedPed);
            }
        }

        private void ApplyCritEffect(Entity entity,
                                     ref Crits crits,
                                     ref ConvertedPed convertedPed,
                                     Crits.Effects newCrit,
                                     bool message) {
            foreach ((Crits.Effects type, BaseCrit action) in critEffects) {
                if (newCrit != type || crits.HasActive(type)) {
                    continue;
                }

#if DEBUG
                sharedData.logger.WriteInfo($"Apply crit effect for {convertedPed.name} - {type}");
#endif

                if (message) {
                    action.ShowCritMessage(ref convertedPed);
                }

                action.Apply(entity, ref convertedPed);
                crits.activeEffects |= type;
                crits.requestBodyPart = default;
            }
        }

        private void IterateCritEffects(Entity entity, ref Crits crits, ref ConvertedPed convertedPed) {
            foreach ((Crits.Effects type, BaseCrit action) in critEffects) {
                if (crits.HasActive(type)) {
                    action.EveryFrame(entity, ref convertedPed);
                }
            }
        }

        private void CancelAllCritEffects(Entity entity, ref Crits crits, ref ConvertedPed convertedPed) {
            foreach ((Crits.Effects type, BaseCrit action) in critEffects) {
                if (crits.HasActive(type)) {
                    action.Cancel(entity, ref convertedPed);
                }
            }

            entity.RemoveComponent<Crits>();
        }
    }
}