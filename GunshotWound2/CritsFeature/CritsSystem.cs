namespace GunshotWound2.CritsFeature {
    using HealthFeature;
    using HitDetection;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class CritsSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly (Crits.Types, BaseCrit)[] critActions;

        private readonly Crits.Types[] upperBodyCrits = {
            Crits.Types.SpineDamaged, Crits.Types.LungsDamaged, Crits.Types.HeartDamaged,
        };

        private readonly Crits.Types[] lowerBodyCrits = {
            Crits.Types.SpineDamaged, Crits.Types.StomachDamaged, Crits.Types.GutsDamaged,
        };

        private Filter pedsWithCrits;
        private Stash<Crits> critsStash;
        private Stash<ConvertedPed> pedStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;

        public World World { get; set; }

        public CritsSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            critActions = new (Crits.Types, BaseCrit)[] {
                (Crits.Types.ArmsDamaged, new ArmsCrit(this.sharedData)),
                (Crits.Types.GutsDamaged, new GutsCrit(this.sharedData)),
                (Crits.Types.HeartDamaged, new HeartCrit(this.sharedData)),
                (Crits.Types.LegsDamaged, new LegsCrit(this.sharedData)),
                (Crits.Types.LungsDamaged, new LungsCrit(this.sharedData)),
                (Crits.Types.SpineDamaged, new SpineCrit(this.sharedData)),
                (Crits.Types.StomachDamaged, new StomachCrit(this.sharedData)),
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
                    CancelAllCrits(entity, ref crits, ref convertedPed);
                } else if (crits.requestBodyPart != PedHitData.BodyParts.Nothing) {
                    Crits.Types newCrit = GetRandomCritFor(crits.requestBodyPart);
#if DEBUG
                    sharedData.logger.WriteInfo($"Random crit {newCrit} for {crits.requestBodyPart} at {convertedPed.name}");
#endif

                    if (newCrit == Crits.Types.SpineDamaged && !sharedData.mainConfig.WoundConfig.RealisticNervesDamage) {
                        ApplyCrit(entity, ref crits, ref convertedPed, Crits.Types.ArmsDamaged);
                        ApplyCrit(entity, ref crits, ref convertedPed, Crits.Types.LegsDamaged);
                    } else {
                        ApplyCrit(entity, ref crits, ref convertedPed, newCrit);
                    }

                    crits.requestBodyPart = default;
                }
            }
        }

        public void Dispose() {
            foreach (Entity entity in pedsWithCrits) {
                ref Crits crits = ref critsStash.Get(entity);
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                CancelAllCrits(entity, ref crits, ref convertedPed);
            }
        }

        private Crits.Types GetRandomCritFor(PedHitData.BodyParts bodyPart) {
            switch (bodyPart) {
                case PedHitData.BodyParts.UpperBody: {
                    int index = sharedData.random.Next(0, upperBodyCrits.Length);
                    return upperBodyCrits[index];
                }

                case PedHitData.BodyParts.LowerBody: {
                    int index = sharedData.random.Next(0, lowerBodyCrits.Length);
                    return lowerBodyCrits[index];
                }

                case PedHitData.BodyParts.Arm: return Crits.Types.ArmsDamaged;
                case PedHitData.BodyParts.Leg: return Crits.Types.LegsDamaged;
                default:                       return default;
            }
        }

        private void ApplyCrit(Entity entity, ref Crits crits, ref ConvertedPed convertedPed, Crits.Types newCrit) {
            foreach ((Crits.Types type, BaseCrit action) in critActions) {
                if (newCrit != type || crits.HasActive(type)) {
                    continue;
                }

#if DEBUG
                sharedData.logger.WriteInfo($"Apply crit for {convertedPed.name} - {type}");
#endif

                action.ShowCritMessage(ref convertedPed);
                action.Apply(entity, ref convertedPed);
                crits.active |= type;
                crits.requestBodyPart = default;

                convertedPed.thisPed.IsPainAudioEnabled = true;
                int pain = sharedData.random.IsTrueWithProbability(0.5f) ? 6 : 7;
                PedEffects.PlayPain(convertedPed.thisPed, pain);
            }
        }

        private void CancelAllCrits(Entity entity, ref Crits crits, ref ConvertedPed convertedPed) {
            foreach ((Crits.Types type, BaseCrit action) in critActions) {
                if (crits.HasActive(type)) {
                    action.Cancel(entity, ref convertedPed);
                }
            }

            entity.RemoveComponent<Crits>();
        }
    }
}