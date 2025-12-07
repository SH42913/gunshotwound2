// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.PedsFeature {
    using Scellecs.Morpeh;
    using RagdollType = GTA.RagdollType;

    public sealed class RagdollSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;

        public World World { get; set; }

        public RagdollSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                Process(ref pedStash.Get(entity));
            }
        }

        private void Process(ref ConvertedPed convertedPed) {
            GTA.Ped ped = convertedPed.thisPed;
            bool inRagdoll = ped.IsRagdoll;
            CallAfterRagdollAction(ref convertedPed, inRagdoll);
            convertedPed.isRagdoll = inRagdoll;

            if (convertedPed.ragdollReset) {
                ResetRagdoll(ref convertedPed);
                return;
            }

            ref (int time, RagdollType type) ragdollRequest = ref convertedPed.ragdollRequest;
            if (convertedPed.permanentRagdoll && !inRagdoll && ragdollRequest.time >= 0) {
#if DEBUG && DEBUG_EVERY_FRAME
                sharedData.logger.WriteInfo($"Force permanent ragdoll to {convertedPed.name}, task={ped.IsRunningRagdollTask}");
#endif
                ragdollRequest = (-1, RagdollType.Relax);
                convertedPed.ragdollRequest = ragdollRequest;
            }

            if (ragdollRequest.time == 0) {
                TryPlayNaturalMotion(ref convertedPed);
                return;
            }

            if (inRagdoll) {
                SkipTime(ref ragdollRequest);
                return;
            }

            // if (RagdollIsBlocked(ref convertedPed)) {
            //     return;
            // }

            ApplyRagdoll(ref convertedPed, ref ragdollRequest);
            convertedPed.ragdollRequest = default;
            convertedPed.isRagdoll = true;
        }

//         private bool RagdollIsBlocked(ref ConvertedPed convertedPed) {
//             if (!convertedPed.HasForcedAnimation()) {
//                 return convertedPed.thisPed.IsInVehicle();
//             }
//
//             if (PedEffects.IsPlayingAnimation(convertedPed.thisPed, convertedPed.forcedAnimation)) {
//                 return true;
//             } else {
// #if DEBUG
//                 sharedData.logger.WriteInfo("Forced animation is finished");
// #endif
//                 convertedPed.forcedAnimation = default;
//                 return false;
//             }
//         }

        private void ApplyRagdoll(ref ConvertedPed convertedPed, ref (int time, RagdollType type) ragdollRequest) {
#if DEBUG && DEBUG_EVERY_FRAME
            var time = ragdollRequest.time.ToString();
            sharedData.logger.WriteInfo($"Ragdoll for {convertedPed.name} time={time}");
#endif

            // ReSharper disable once InconsistentNaming
            bool startedNM = !convertedPed.hasSpineDamage && TryPlayNaturalMotion(ref convertedPed);
            if (!startedNM) {
                convertedPed.thisPed.Ragdoll(ragdollRequest.time, ragdollRequest.type);
            }
        }

        private bool TryPlayNaturalMotion(ref ConvertedPed convertedPed) {
            if (convertedPed.requestedNmHelper == null) {
                return false;
            }

            if (convertedPed.activeNmHelper != null) {
#if DEBUG && DEBUG_EVERY_FRAME
                sharedData.logger.WriteInfo($"Stopping NM helper: {convertedPed.activeNmHelper.GetType().FullName}");
#endif
                convertedPed.activeNmHelper.Stop();
                convertedPed.thisPed.CancelRagdoll();
            }

#if DEBUG && DEBUG_EVERY_FRAME
            sharedData.logger.WriteInfo($"Starting NM helper: {convertedPed.requestedNmHelper.GetType().FullName}");
#endif

            convertedPed.activeNmHelper = convertedPed.requestedNmHelper;
            convertedPed.requestedNmHelper = null;

            int ragdollTime = convertedPed.ragdollRequest.time;
            if (ragdollTime <= 0) {
                convertedPed.activeNmHelper.Start();
            } else {
                convertedPed.activeNmHelper.Start(ragdollTime);
            }

            return true;
        }

        private void CallAfterRagdollAction(ref ConvertedPed convertedPed, bool inRagdoll) {
            if (convertedPed.isRagdoll == inRagdoll || inRagdoll || convertedPed.afterRagdollAction == null) {
                return;
            }
#if DEBUG
            string methodName = convertedPed.afterRagdollAction.Method.Name;
            sharedData.logger.WriteInfo($"Applying after ragdoll action {methodName}");
#endif
            convertedPed.afterRagdollAction.Invoke(ref convertedPed);
            convertedPed.afterRagdollAction = null;
        }

        private void ResetRagdoll(ref ConvertedPed convertedPed) {
            if (convertedPed.isRagdoll) {
#if DEBUG
                sharedData.logger.WriteInfo($"Restore {convertedPed.name} from ragdoll");
#endif
                convertedPed.thisPed.CancelRagdoll();
            }

            convertedPed.ragdollReset = false;
            convertedPed.permanentRagdoll = false;
            convertedPed.ragdollRequest = default;
            convertedPed.activeNmHelper = null;
        }

        private void SkipTime(ref (int time, RagdollType type) ragdollRequest) {
            if (ragdollRequest.time <= 0) {
                return;
            }

            int newTime = ragdollRequest.time - sharedData.deltaTimeInMs;
            ragdollRequest.time = newTime > 0 ? newTime : 0;
        }

        public void Dispose() {
            foreach (Entity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (convertedPed.thisPed.IsRagdoll) {
                    convertedPed.thisPed.CancelRagdoll();
                }
            }
        }
    }
}