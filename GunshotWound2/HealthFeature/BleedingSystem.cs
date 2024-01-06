﻿namespace GunshotWound2.HealthFeature {
    using System;
    using System.Collections.Generic;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class BleedingSystem : ISystem {
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

                health.diff -= bleeding.severity * deltaTime;
                bleeding.severity -= health.bleedingHealRate * deltaTime;
            }
        }

        private void ProcessBleeding(Entity entity, ref Bleeding bleeding, ref Health health) {
            bleeding.canBeBandaged = !bleeding.isInternal
                                     && sharedData.mainConfig.WoundConfig.IsBleedingCanBeBandaged(bleeding.severity);

            health.bleedingWounds ??= new HashSet<Entity>(4);
            health.bleedingWounds.Add(entity);
            health.bleedingToBandage = null;
            bleeding.isProcessed = true;
        }

        private void RefreshBleedingToBandage() {
            foreach (Entity entity in peds) {
                ref Health health = ref healthStash.Get(entity);
                if (pedsStash.Get(entity).isPlayer && health.bleedingToBandage.IsNullOrDisposed()) {
                    DetectBleedingToBandage(ref health);
                }
            }
        }

        private void DetectBleedingToBandage(ref Health health) {
            if (health.bleedingWounds == null) {
                return;
            }

            float maxBleeding = 0f;
            Entity mostDangerWound = null;
            foreach (Entity entity in health.bleedingWounds) {
                ref Bleeding bleeding = ref bleedingStash.Get(entity);
                if (!bleeding.canBeBandaged) {
                    continue;
                }

                if (bleeding.severity > maxBleeding) {
                    maxBleeding = bleeding.severity;
                    mostDangerWound = entity;
                }
            }

            health.bleedingToBandage = mostDangerWound;
        }
    }
}