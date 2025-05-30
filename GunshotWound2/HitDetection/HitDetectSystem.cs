namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class HitDetectSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter convertedPeds;
        private Filter justConvertedPeds;
        private Stash<ConvertedPed> convertedStash;

        public Scellecs.Morpeh.World World { get; set; }

        public HitDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            convertedPeds = World.Filter.With<ConvertedPed>().Without<JustConvertedEvent>();
            justConvertedPeds = World.Filter.With<JustConvertedEvent>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            ProcessJustDamagedPeds();
            ProcessCommonPeds();
        }

        private void ProcessJustDamagedPeds() {
            if (!sharedData.mainConfig.pedsConfig.ScanOnlyDamaged) {
                return;
            }

            foreach (Scellecs.Morpeh.Entity entity in justConvertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                if (convertedPed.isPlayer) {
                    continue;
                }

                Ped ped = convertedPed.thisPed;
                if (!ped.Exists() || !ped.IsAlive || ped.IsInvincible) {
                    continue;
                }

                entity.SetComponent(new PedHitData());

#if DEBUG
                sharedData.logger.WriteInfo($"Detect damage at {convertedPed.name} (just converted case)");
#endif
            }
        }

        private void ProcessCommonPeds() {
            foreach (Scellecs.Morpeh.Entity entity in convertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (!ped.Exists() || ped.IsInvincible || PedEffects.IsPedInWrithe(ped)) {
                    continue;
                }

                var wasKilledByTakedown = false;
                if (ped.IsDead) {
                    wasKilledByTakedown = ped.WasKilledByTakedown;
                    if (wasKilledByTakedown) {
#if DEBUG
                        sharedData.logger.WriteInfo("Resurrecting ped after takedown");
#endif
                        ped.Resurrect();
                        ped.Health = convertedPed.lastFrameHealth - 1;
                        ped.SetConfigFlag(PedConfigFlagToggles.KilledByTakedown, false);
                    } else {
                        continue;
                    }
                }

                int healthDiff = convertedPed.lastFrameHealth - ped.Health;
                int armorDiff = convertedPed.lastFrameArmor - ped.Armor;
                if (healthDiff <= 0 && armorDiff <= 0) {
                    continue;
                }

                entity.SetComponent(new PedHitData {
                    healthDiff = healthDiff,
                    armorDiff = armorDiff,
                    afterTakedown = wasKilledByTakedown,
                });

#if DEBUG
                sharedData.logger.WriteInfo($"Detect damage at {convertedPed.name}");
                sharedData.logger.WriteInfo($"healthDiff = {healthDiff.ToString()}, armorDiff = {armorDiff.ToString()}");
#endif
            }
        }
    }
}