namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class HitDetectSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter convertedPeds;
        private Stash<ConvertedPed> convertedStash;

        public EcsWorld World { get; set; }

        public HitDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            convertedPeds = World.Filter.With<ConvertedPed>().Without<JustConvertedEvent>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in convertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (!ped.IsValid() || ped.IsInvincible || PedEffects.IsPedInWrithe(ped)) {
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
                        ped.Ragdoll(sharedData.mainConfig.woundConfig.TakedownRagdollDurationMs, RagdollType.Balance);
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