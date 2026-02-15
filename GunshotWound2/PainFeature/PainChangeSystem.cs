// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.PainFeature {
    using System;
    using Configs;
    using GTA;
    using HealthFeature;
    using HitDetection;
    using PedsFeature;
    using PlayerFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PainChangeSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter pedsWithPain;
        private Stash<ConvertedPed> pedStash;
        private Stash<Pain> painStash;
        private Stash<TotallyHealedEvent> totallyHealedStash;
        private Stash<PainkillersEffect> painkillersStash;

        public EcsWorld World { get; set; }

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
            foreach (EcsEntity entity in pedsWithPain) {
                ref Pain pain = ref painStash.Get(entity);
                if (totallyHealedStash.Has(entity)) {
                    pain.amount = 0f;
                    pain.diff = 0f;
                    pain.delayedDiff = 0f;
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref PainkillersEffect painkillersEffect = ref painkillersStash.Get(entity, out bool painkillersActive);
                if (pain.diff > 0f) {
                    bool skipDelay = pain.dontDelayDiff;
                    if (!skipDelay) {
                        DelayPain(ref pain);
                    }

                    if (painkillersActive) {
                        float painToMax = 0.99f * (pain.max - pain.amount);
                        pain.diff = Math.Min(pain.diff, painToMax);
                    }

                    ApplyPain(entity, ref convertedPed, ref pain);
                } else if (pain.delayedDiff > 0 && !painkillersActive) {
                    UpdateDelayedPain(ref pain, deltaTime);
                    ApplyPain(entity, ref convertedPed, ref pain);
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
            if (pain.TooMuchPain()) {
                return;
            }

            float diff = pain.diff;
            if (diff <= 0f && diff < pain.delayedDiff) {
                return;
            }

            float toDelay = diff * sharedData.mainConfig.woundConfig.DelayedPainPercent;
            if (toDelay < 1f) {
                return;
            }

            pain.diff -= toDelay;
            pain.delayedDiff += toDelay;

#if DEBUG && DEBUG_EVERY_FRAME
            sharedData.logger.WriteInfo($"{toDelay} of pain({diff}) delayed, total:{pain.delayedDiff}");
#endif
        }

        private void UpdateDelayedPain(ref Pain pain, float deltaTime) {
            if (pain.TooMuchPain()) {
                ApplyAllDelayedPain(ref pain);
                return;
            }

            if (pain.delayedDiff <= 0f) {
                return;
            }

            float painToApply = sharedData.mainConfig.woundConfig.DelayedPainSpeed * deltaTime;
            if (painToApply < pain.delayedDiff) {
                pain.diff += painToApply;
                pain.delayedDiff -= painToApply;
            } else {
                ApplyAllDelayedPain(ref pain);
            }
        }

        private static void ApplyAllDelayedPain(ref Pain pain) {
            pain.diff = pain.delayedDiff;
            pain.delayedDiff = 0f;
        }

        private void ApplyPain(EcsEntity entity, ref ConvertedPed convertedPed, ref Pain pain) {
#if DEBUG && DEBUG_EVERY_FRAME
            sharedData.logger.WriteInfo($"Applying pain, current:{pain.amount} diff:{pain.diff}");
#endif

            if (convertedPed.isPlayer && PlayerEffects.InRampageScenarioUsedBy(convertedPed.thisPed)) {
                ReducePainDiffForRampage(ref pain);
#if DEBUG && DEBUG_EVERY_FRAME
                sharedData.logger.WriteInfo("Reduced pain due to Rampage");
#endif
            }

            PlayPainEffects(entity, ref convertedPed, ref pain);

            bool wasTooMuch = pain.TooMuchPain();
            float diff = pain.diff;
            pain.amount += diff;
            pain.diff = 0f;

            if (!wasTooMuch && pain.TooMuchPain()) {
                EnsurePainOverflow(ref pain);
            }

#if DEBUG && DEBUG_EVERY_FRAME
            sharedData.logger.WriteInfo($"Increased pain of {convertedPed.name} to {pain.amount}");
#endif
        }

        private void ReducePainDiffForRampage(ref Pain pain) {
            pain.diff = sharedData.mainConfig.playerConfig.RampagePainMult * pain.diff;
        }

        private static void EnsurePainOverflow(ref Pain pain) {
            const float ensurePainOverflow = 0.05f;
            float percent = pain.Percent();
            if (percent - 1f < ensurePainOverflow) {
                pain.diff += ensurePainOverflow * pain.max;
            }
        }

        private void UpdatePainkillersEffect(EcsEntity entity,
                                             in ConvertedPed convertedPed,
                                             ref Pain pain,
                                             ref PainkillersEffect painkillersEffect,
                                             float deltaTime) {
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

        private static void RecoverPain(ref Pain pain, ref PainkillersEffect painkillersEffect, float deltaTime) {
            pain.amount -= painkillersEffect.rate * deltaTime;
            pain.amount -= pain.recoveryRate * deltaTime;
            pain.amount = Math.Max(pain.amount, 0f);
        }

        private void PlayPainEffects(EcsEntity entity, ref ConvertedPed convertedPed, ref Pain pain) {
            if (convertedPed.hasSpineDamage) {
                return;
            }

            Ped ped = convertedPed.thisPed;
            if (!ped.IsValid()) {
                return;
            }

            int painAnimIndex = sharedData.random.Next(1, 7);
            PedEffects.PlayFacialAnim(ped, $"pain_{painAnimIndex.ToString()}", convertedPed.isMale);

            WoundConfig woundConfig = sharedData.mainConfig.woundConfig;
            float painfulThreshold = woundConfig.PainfulWoundPercent * pain.max;
            if (pain.diff >= painfulThreshold) {
#if DEBUG
                sharedData.logger.WriteInfo($"Painful wound {pain.diff.ToString("F")} at {convertedPed.name}");
#endif

                ped.PlayAmbientSpeech("PAIN_RAPIDS", SpeechModifier.ForceShouted);
                if (woundConfig.RagdollOnPainfulWound) {
                    convertedPed.RequestRagdoll(timeInMs: 1500);
                }

                ref PedHitData hitData = ref entity.GetComponent<PedHitData>(out bool hasHitData);
                if (convertedPed.isPlayer) {
                    sharedData.cameraService.PlayPainfulWoundEffect();
                } else if (hasHitData && hitData.bodyPart.IsValid && hitData.bodyPart.IsRightArm()) {
                    ped.Weapons.Drop();
                }
            }
        }
    }
}