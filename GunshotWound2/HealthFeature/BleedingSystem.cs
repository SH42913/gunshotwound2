// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.HealthFeature {
    using System;
    using System.Collections.Generic;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class BleedingSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Stash<Bleeding> bleedingStash;
        private Stash<ConvertedPed> pedsStash;
        private Stash<Health> healthStash;

        private Filter bleedingWounds;
        private Filter peds;

        public BleedingSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public World World { get; set; }

        public void OnAwake() {
            bleedingStash = World.GetStash<Bleeding>();
            healthStash = World.GetStash<Health>();
            pedsStash = World.GetStash<ConvertedPed>();

            bleedingWounds = World.Filter.With<Bleeding>();
            peds = World.Filter.With<ConvertedPed>().With<Health>();
        }

        public void OnUpdate(float deltaTime) {
            UpdateBleedingWounds(deltaTime);
            RefreshBleedingToBandage();
        }

        void IDisposable.Dispose() { }

        private void UpdateBleedingWounds(float deltaTime) {
            foreach (Entity entity in bleedingWounds) {
                ref Bleeding bleeding = ref bleedingStash.Get(entity);
                if (bleeding.target.IsNullOrDisposed()) {
                    World.RemoveEntity(entity);
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedsStash.Get(bleeding.target);
                ref Health health = ref healthStash.Get(bleeding.target, out bool hasHealth);
                if (!hasHealth) {
                    sharedData.logger.WriteWarning($"Can't update bleeding {bleeding.name} at {convertedPed.name}");
                    World.RemoveEntity(entity);
                    continue;
                }

                if (health.isDead) {
                    health.bleedingWounds?.Remove(entity);
                    World.RemoveEntity(entity);
                    continue;
                }

                if (!bleeding.isProcessed) {
                    ProcessBleeding(entity, ref bleeding, ref health);
#if DEBUG
                    sharedData.logger.WriteInfo($"Bleeding {bleeding.name} at {convertedPed.name} was processed");
#endif
                }

                if (bleeding.severity <= 0f || bleeding.target.Has<TotallyHealedEvent>()) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Bleeding {bleeding.name} at {convertedPed.name} was healed");
#endif
                    health.bleedingWounds.Remove(entity);
                    World.RemoveEntity(entity);
                    continue;
                }

                health.DealDamage(bleeding.severity * deltaTime, reason: null);

                if (!bleeding.isTrauma) {
                    bleeding.severity -= health.bleedingHealRate * deltaTime;
                }
            }
        }

        private void ProcessBleeding(Entity entity, ref Bleeding bleeding, ref Health health) {
            health.bleedingWounds ??= new HashSet<Entity>(4);
            health.bleedingWounds.Add(entity);
            health.bleedingToBandage = null;
            bleeding.isProcessed = true;
            bleeding.processedTime = GTA.Game.GameTime;
        }

        private void RefreshBleedingToBandage() {
            foreach (Entity pedEntity in peds) {
                ref Health health = ref healthStash.Get(pedEntity);
                if (health.bleedingToBandage.IsNullOrDisposed()) {
                    DetectBleedingToBandage(pedEntity, ref health);
                }
            }
        }

        private void DetectBleedingToBandage(Entity pedEntity, ref Health health) {
            if (!health.HasBleedingWounds()) {
                return;
            }

            (Entity ent, float severity) mostDangerWound = default;
            (Entity ent, float severity) woundToBandage = default;
            foreach (Entity entity in health.bleedingWounds) {
                ref Bleeding bleeding = ref bleedingStash.Get(entity);

                if (mostDangerWound.ent.IsNullOrDisposed() || bleeding.severity > mostDangerWound.severity) {
                    mostDangerWound.ent = entity;
                    mostDangerWound.severity = bleeding.severity;
                }

                bool ableToBandage = !bleeding.isTrauma;
                if (ableToBandage && (woundToBandage.ent.IsNullOrDisposed() || bleeding.severity > woundToBandage.severity)) {
                    woundToBandage.ent = entity;
                    woundToBandage.severity = bleeding.severity;
                }
            }

            health.mostDangerousBleeding = mostDangerWound.ent;
            health.bleedingToBandage = woundToBandage.ent;
#if DEBUG
            if (!woundToBandage.ent.IsNullOrDisposed()) {
                string pedName = pedEntity.GetComponent<ConvertedPed>().name;
                string woundName = woundToBandage.ent.GetComponent<Bleeding>().name;
                sharedData.logger.WriteInfo($"Updated bleedingToBandage for {pedName} to {woundName}");
            }
#endif
        }
    }
}