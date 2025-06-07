namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using HealthFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using States;

    public sealed class PainChangeSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter pedsWithPain;
        private Stash<ConvertedPed> pedStash;
        private Stash<Pain> painStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;
        private Stash<PainkillersEffect> painkillersStash;

        public World World { get; set; }

        public PainChangeSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            pedsWithPain = World.Filter.With<ConvertedPed>().With<Pain>();
            pedStash = World.GetStash<ConvertedPed>();
            painStash = World.GetStash<Pain>();
            totallyHealedStash = World.GetStash<TotallyHealedEvent>();
            painkillersStash = World.GetStash<PainkillersEffect>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in pedsWithPain) {
                ref Pain pain = ref painStash.Get(entity);
                if (totallyHealedStash.Has(entity)) {
                    pain.amount = 0f;
                    pain.diff = 0f;
                    pain.delayedDiff = 0f;
                    pain.currentState = null;
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref PainkillersEffect painkillersEffect = ref painkillersStash.Get(entity, out bool painkillersActive);
                if (pain.diff > 0f) {
                    if (!painkillersActive) {
                        DelayPain(ref pain);
                    }

#if DEBUG && DEBUG_EVERY_FRAME
                    float applied = ApplyPain(ref convertedPed, ref pain);
                    sharedData.logger.WriteInfo($"Increased pain for {applied} to {pain.amount} at {convertedPed.name}");
#else
                    ApplyPain(ref convertedPed, ref pain);
#endif
                } else if (pain.delayedDiff > 0) {
                    UpdateDelayedPain(ref pain, deltaTime);
                    ApplyPain(ref convertedPed, ref pain);
                } else if (pain.HasPain()) {
                    RecoverPain(ref pain, ref painkillersEffect, deltaTime);
                }

                if (painkillersActive) {
                    UpdatePainkillersEffect(entity, convertedPed, ref pain, ref painkillersEffect, deltaTime);
                }
            }
        }

        void IDisposable.Dispose() { }

        private void DelayPain(ref Pain pain) {
            if (IsUnbearableState(pain)) {
                return;
            }

            if (pain.diff <= 0f && pain.diff < pain.delayedDiff) {
                return;
            }

            float toDelay = pain.diff * sharedData.mainConfig.woundConfig.DelayedPainPercent;
            if (toDelay < 1f) {
                return;
            }

            pain.diff -= toDelay;
            pain.delayedDiff += toDelay;

#if DEBUG
            sharedData.logger.WriteInfo($"{toDelay.ToString("F2")} of pain delayed");
#endif
        }

        private void UpdateDelayedPain(ref Pain pain, float deltaTime) {
            if (IsUnbearableState(pain)) {
                ApplyAllDelayedPain(ref pain);
                return;
            }

            if (pain.delayedDiff <= 0f) {
                return;
            }

            if (deltaTime < pain.delayedDiff) {
                pain.diff += deltaTime;
                pain.delayedDiff -= deltaTime;
            } else {
                ApplyAllDelayedPain(ref pain);
            }
        }

        private static void ApplyAllDelayedPain(ref Pain pain) {
            pain.diff = pain.delayedDiff;
            pain.delayedDiff = 0f;
        }

        private float ApplyPain(ref ConvertedPed convertedPed, ref Pain pain) {
            PlayPainEffects(ref convertedPed, ref pain);

            float diff = pain.diff;
            pain.amount += diff;
            pain.diff = 0f;
            return diff;
        }

        private void UpdatePainkillersEffect(Entity entity,
                                             in ConvertedPed convertedPed,
                                             ref Pain pain,
                                             ref PainkillersEffect painkillersEffect,
                                             float deltaTime) {
            pain.delayedDiff = 0f;
            pain.amount = Math.Min(pain.amount, pain.max);

            painkillersEffect.remainingTime -= deltaTime;
            if (painkillersEffect.remainingTime <= 0f) {
                painkillersStash.Remove(entity);
#if DEBUG
                sharedData.logger.WriteInfo("Painkillers effect has ended");
#endif
                sharedData.cameraService.SetPainkillersEffect(false);
            } else if (convertedPed.isPlayer && !painkillersEffect.effectIsActive) {
                sharedData.cameraService.SetPainkillersEffect(true);
                painkillersEffect.effectIsActive = true;
            }
        }

        private void RecoverPain(ref Pain pain, ref PainkillersEffect painkillersEffect, float deltaTime) {
            pain.amount -= painkillersEffect.rate * deltaTime;
            pain.amount -= pain.recoveryRate * deltaTime;
            pain.amount = Math.Max(pain.amount, 0f);
        }

        private void PlayPainEffects(ref ConvertedPed convertedPed, ref Pain pain) {
            if (convertedPed.hasSpineDamage) {
                return;
            }

            int painAnimIndex = sharedData.random.Next(1, 7);
            PedEffects.PlayFacialAnim(convertedPed.thisPed, $"pain_{painAnimIndex.ToString()}", convertedPed.isMale);

            WoundConfig woundConfig = sharedData.mainConfig.woundConfig;
            float painfulThreshold = woundConfig.PainfulWoundPercent * pain.max;
            if (pain.diff >= painfulThreshold) {
#if DEBUG
                sharedData.logger.WriteInfo($"Painful wound {pain.diff.ToString("F")} at {convertedPed.name}");
#endif

                convertedPed.thisPed.PlayAmbientSpeech("PAIN_RAPIDS");
                if (woundConfig.RagdollOnPainfulWound) {
                    convertedPed.RequestRagdoll(timeInMs: 1500);
                }

                if (convertedPed.isPlayer) {
                    sharedData.cameraService.PlayPainfulWoundEffect();
                } else if (convertedPed.thisPed.IsOnBike) {
                    convertedPed.thisPed.Task.LeaveVehicle(GTA.LeaveVehicleFlags.BailOut);
                }
            }
        }

        private static bool IsUnbearableState(in Pain pain) {
            return pain.currentState is UnbearablePainState || pain.currentState is DeadlyPainState;
        }
    }
}