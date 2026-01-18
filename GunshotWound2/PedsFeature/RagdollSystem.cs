namespace GunshotWound2.PedsFeature {
    using GTA.NaturalMotion;
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
            if (ped.IsInVehicle()) {
                return;
            }

            bool inRagdollCurrentFrame = ped.IsRagdoll;
            HandleAfterRagdollAction(ref convertedPed, inRagdollCurrentFrame);
            convertedPed.isRagdoll = inRagdollCurrentFrame;

            if (convertedPed.ragdollReset) {
                ResetRagdoll(ref convertedPed);
                return;
            }

            bool isRealRagdoll = GetRealPedRagdoll(convertedPed);
            EnsurePermanentRagdoll(ref convertedPed, isRealRagdoll);
            ReplaceRequestWithBodyRelaxIfSpineDamage(ref convertedPed);

            bool newHelperRequest = convertedPed.requestedNmHelper != null;
            bool hasActiveRequest = convertedPed.ragdollRequest.time != 0 || newHelperRequest;
            if (!hasActiveRequest) {
                return;
            }

            if (!convertedPed.isRagdoll || newHelperRequest) {
                ExecuteNewRagdoll(ref convertedPed);
            } else {
                HandleActiveRagdoll(ref convertedPed);
            }
        }

        private void ReplaceRequestWithBodyRelaxIfSpineDamage(ref ConvertedPed convertedPed) {
            if (!convertedPed.hasSpineDamage) {
                return;
            }

            bool isActiveRelax = convertedPed.activeNmHelper is BodyRelaxHelper;
            bool isRequestingRelax = convertedPed.requestedNmHelper is BodyRelaxHelper;
            if (!isActiveRelax && !isRequestingRelax) {
#if DEBUG
                sharedData.logger.WriteInfo($"Spine damaged: Forcing BodyRelaxHelper for {convertedPed.name}");
#endif
                convertedPed.requestedNmHelper = new BodyRelaxHelper(convertedPed.thisPed);
            } else if (isActiveRelax && convertedPed.requestedNmHelper != null && !isRequestingRelax) {
#if DEBUG
                sharedData.logger.WriteInfo($"Spine damaged: Ignoring non-relax helper for {convertedPed.name}");
#endif
                convertedPed.requestedNmHelper = null;
            }
        }

        private bool GetRealPedRagdoll(in ConvertedPed convertedPed) {
            bool inRagdollOrTask = convertedPed.isRagdoll || PedEffects.IsRunningRagdollTask(convertedPed.thisPed);
            return inRagdollOrTask && !PedEffects.IsPedGettingUp(convertedPed.thisPed);
        }

        private void EnsurePermanentRagdoll(ref ConvertedPed convertedPed, bool isRealRagdoll) {
            if (!convertedPed.permanentRagdoll || isRealRagdoll) {
                return;
            }

            if (convertedPed.ragdollRequest.time >= 0) {
#if DEBUG
                sharedData.logger.WriteInfo($"Force permanent ragdoll to {convertedPed.name}");
#endif
                convertedPed.ragdollRequest = (-1, RagdollType.Relax);
            }
        }

        private void HandleActiveRagdoll(ref ConvertedPed convertedPed) {
            SkipTime(ref convertedPed.ragdollRequest);

            if (!convertedPed.permanentRagdoll && convertedPed.ragdollRequest.time == 0) {
                convertedPed.ragdollRequest = default;
            }
        }

        private void ExecuteNewRagdoll(ref ConvertedPed convertedPed) {
            ref (int time, RagdollType type) request = ref convertedPed.ragdollRequest;

#if DEBUG
            sharedData.logger.WriteInfo($"Apply Ragdoll for {convertedPed.name} time={request.time}");
#endif

            bool ragdollStarted = TryPlayNaturalMotion(ref convertedPed);
            if (!ragdollStarted) {
                convertedPed.thisPed.Ragdoll(request.time, request.type);
            }

            if (!convertedPed.permanentRagdoll) {
                convertedPed.ragdollRequest = default;
            }
        }

        private bool TryPlayNaturalMotion(ref ConvertedPed convertedPed) {
            if (convertedPed.requestedNmHelper == null) {
                return false;
            }

            if (convertedPed.activeNmHelper != null) {
#if DEBUG
                sharedData.logger.WriteInfo($"Switching NM helper: stopping {convertedPed.activeNmHelper.GetType().Name}");
#endif
                convertedPed.activeNmHelper.Stop();
            }

            convertedPed.activeNmHelper = convertedPed.requestedNmHelper;
            convertedPed.requestedNmHelper = null;

#if DEBUG
            sharedData.logger.WriteInfo($"Starting NM helper: {convertedPed.activeNmHelper.GetType().Name}");
#endif

            int duration = convertedPed.ragdollRequest.time;
            convertedPed.thisPed.Ragdoll(duration, RagdollType.ScriptControl);
            convertedPed.activeNmHelper.Start(duration);
            return true;
        }

        private void HandleAfterRagdollAction(ref ConvertedPed convertedPed, bool currentFrameRagdoll) {
            if (convertedPed.afterRagdollAction == null) {
                return;
            }

            bool wasInRagdoll = convertedPed.isRagdoll;
            if (wasInRagdoll == currentFrameRagdoll || currentFrameRagdoll) {
                return;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Applying after ragdoll action {convertedPed.afterRagdollAction.Method.Name}");
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

            if (convertedPed.activeNmHelper != null) {
                convertedPed.activeNmHelper.Stop();
                convertedPed.activeNmHelper = null;
            }
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
                ResetRagdoll(ref pedStash.Get(entity));
            }
        }
    }
}