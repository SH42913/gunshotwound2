namespace GunshotWound2.StatusFeature {
    using System;
    using GTA;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Statuses;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PedStatusSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly IPedStatus[] statuses;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;
        private Stash<Pain> painStash;
        private Stash<Health> healthStash;

        public EcsWorld World { get; set; }

        public PedStatusSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            statuses = new IPedStatus[] {
                new WarningStatus(sharedData),
                new DistressedStatus(sharedData),
                new CriticalStatus(sharedData),
                new UnconsciousStatus(sharedData),
            };
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<Pain>().With<Health>();
            pedStash = World.GetStash<ConvertedPed>();
            painStash = World.GetStash<Pain>();
            healthStash = World.GetStash<Health>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                ref Health health = ref healthStash.Get(entity);
                if (!convertedPed.thisPed.IsValid() || health.isDead) {
                    continue;
                }

                if (convertedPed.isPlayer && entity.Has<JustConvertedEvent>()) {
                    sharedData.cameraService.ClearAllEffects();
                    convertedPed.resetMoveSet = true;
                    convertedPed.status = null;
                }

                ref Pain pain = ref painStash.Get(entity);
                int newStatusIndex = GetNewStatusIndex(convertedPed, ref health, ref pain);
                ChangeStatus(entity, ref convertedPed, newStatusIndex);
            }
        }

        public void Dispose() {
            foreach (EcsEntity entity in peds) {
                ChangeStatus(entity, ref pedStash.Get(entity), newStatusIndex: -1);
            }
        }

        private int GetNewStatusIndex(in ConvertedPed convertedPed, ref Health health, ref Pain pain) {
            if (!pain.HasPain() && !health.IsDamaged(convertedPed)) {
                health.statusColor = Notifier.Color.COMMON;
                return -1;
            }

            float healthPercent = health.Percent(convertedPed);
            float painPercent = pain.Percent();

            int healthStatusIndex = -1;
            int painStatusIndex = -1;
            for (var i = 0; i < statuses.Length; i++) {
                IPedStatus status = statuses[i];
                if (painPercent >= status.PainThreshold) {
                    painStatusIndex = i;
                }

                if (healthPercent <= status.HealthThreshold) {
                    healthStatusIndex = i;
                }
            }

            pain.statusColor = painStatusIndex >= 0 ? statuses[painStatusIndex].Color : Notifier.Color.COMMON;
            health.statusColor = healthStatusIndex >= 0 ? statuses[healthStatusIndex].Color : Notifier.Color.COMMON;
            return Math.Max(healthStatusIndex, painStatusIndex);
        }

        private void ChangeStatus(EcsEntity entity,
                                  ref ConvertedPed convertedPed,
                                  int newStatusIndex) {
            int curStatusIndex = convertedPed.status != null ? Array.IndexOf(statuses, convertedPed.status) : -1;
            if (curStatusIndex == newStatusIndex) {
                return;
            }

#if DEBUG
            string currentName = curStatusIndex >= 0 ? statuses[curStatusIndex].GetType().Name : "STABLE";
            string newName = newStatusIndex >= 0 ? statuses[newStatusIndex].GetType().Name : "STABLE";
            sharedData.logger.WriteInfo($"Changed status of {convertedPed.name}: {currentName} => {newName}");
#endif

            int direction = Math.Sign(newStatusIndex - curStatusIndex);
            if (convertedPed.isPlayer && direction < 0) {
                sharedData.notifier.info.QueueMessage(sharedData.localeConfig.PainDecreasedMessage, Notifier.Color.GREEN);
            }

            IPedStatus curStatus = null;
            while (curStatusIndex != newStatusIndex) {
                IPedStatus prevStatus = curStatusIndex >= 0 ? statuses[curStatusIndex] : null;
                curStatusIndex += direction;
                curStatus = curStatusIndex >= 0 ? statuses[curStatusIndex] : null;

                if (direction > 0) {
                    curStatus?.ApplyStatusTo(ref convertedPed);
                } else {
                    prevStatus?.RemoveStatusFrom(ref convertedPed);
                }
            }

            if (curStatus is UnconsciousStatus) {
                entity.AddComponent<UnconsciousVisualRequest>();
            }

            convertedPed.status = curStatus;

            RefreshMoveSet(ref convertedPed, curStatus);
            RefreshMood(ref convertedPed, curStatus);
            RefreshMoveRate(ref convertedPed, curStatus);
            PlaySpeech(ref convertedPed, curStatus);
        }

        private void RefreshMoveSet(ref ConvertedPed convertedPed, IPedStatus status) {
            string[] moveSets = convertedPed.isMale ? status?.MaleMoveSets : status?.FemaleMoveSets;
            if (moveSets != null && moveSets.Length > 0) {
                convertedPed.moveSetRequest = sharedData.random.Next(moveSets);
            } else {
                convertedPed.resetMoveSet = true;
            }
        }

        private void RefreshMood(ref ConvertedPed convertedPed, IPedStatus status) {
            string[] idleAnims = status?.FacialIdleAnims;
            if (idleAnims != null && idleAnims.Length > 0) {
                PedEffects.SetFacialIdleAnim(convertedPed.thisPed, sharedData.random.Next(idleAnims), convertedPed.isMale);
            } else {
                PedEffects.CleanFacialIdleAnim(convertedPed.thisPed);
            }
        }

        private void PlaySpeech(ref ConvertedPed convertedPed, IPedStatus status) {
            string[] speechSet = convertedPed.isPlayer ? status?.PlayerSpeechSet : status?.PedSpeechSet;
            if (speechSet != null && speechSet.Length > 0) {
                string speechName = sharedData.random.Next(speechSet);
                convertedPed.thisPed.PlayAmbientSpeech(speechName, SpeechModifier.InterruptShouted);
            }
        }

        private static void RefreshMoveRate(ref ConvertedPed convertedPed, IPedStatus status) {
            if (convertedPed.hasBrokenLegs) {
                return;
            }

            if (status != null) {
                convertedPed.moveRate = status.MoveRate;
            } else {
                convertedPed.ResetMoveRate();
            }
        }
    }
}