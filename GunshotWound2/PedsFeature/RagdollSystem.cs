namespace GunshotWound2.PedsFeature {
    using GTA.Native;
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
            if (convertedPed.permanentRagdoll && ragdollRequest.time >= 0) {
#if DEBUG
                sharedData.logger.WriteInfo($"Applying permanent ragdoll to {convertedPed.name}");
#endif
                ragdollRequest = (-1, RagdollType.Relax);
            }

            if (ragdollRequest.time == 0) {
                return;
            }

            if (inRagdoll) {
                SkipTime(ref ragdollRequest);
                return;
            }

            bool isPlaying = PedEffects.IsPlayingAnimation(ped, convertedPed.forcedAnimation);
            bool ragdollIsBlocked = ped.IsInVehicle() || isPlaying;
            if (ragdollIsBlocked) {
                return;
            }

            ApplyRagdoll(ref convertedPed, ref ragdollRequest);
            convertedPed.ragdollRequest = default;
            convertedPed.isRagdoll = true;
        }

        private void ApplyRagdoll(ref ConvertedPed convertedPed, ref (int time, RagdollType type) ragdollRequest) {
#if DEBUG
            var time = ragdollRequest.time.ToString();
            sharedData.logger.WriteInfo($"Ragdoll for {convertedPed.name} time={time}");
#endif

            // ReSharper disable once InconsistentNaming
            bool canPlayNM = !convertedPed.hasSpineDamage;
            if (canPlayNM && convertedPed.nmHelper != null) {
#if DEBUG
                sharedData.logger.WriteInfo($"Playing NM helper: {convertedPed.nmHelper.GetType().FullName}");
#endif
                convertedPed.nmHelper.Start();
            } else if (canPlayNM && convertedPed.nmMessages != null) {
#if DEBUG
                sharedData.logger.WriteInfo($"Playing NM messages: {string.Join(" ,", convertedPed.nmMessages)}");
#endif
                Function.Call(Hash.SET_PED_TO_RAGDOLL, convertedPed.thisPed.Handle, 10000, ragdollRequest.time, 1, 1, 1, 0);
                ApplyNaturalMotion(ref convertedPed);
                convertedPed.nmMessages = null;
            } else {
                convertedPed.thisPed.Ragdoll(ragdollRequest.time, ragdollRequest.type);
            }
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

        private static void ApplyNaturalMotion(ref ConvertedPed convertedPed) {
            PedEffects.StopNaturalMotion(convertedPed.thisPed);
            foreach (int message in convertedPed.nmMessages) {
                PedEffects.SetNaturalMotionMessage(convertedPed.thisPed, message);
            }
        }
    }
}