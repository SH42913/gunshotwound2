// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.PedsFeature {
    using GTA;
    using GTA.NaturalMotion;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class RagdollSystem : ILateSystem {
        public const int PERMANENT_RAGDOLL_TIME = -1;

        private readonly SharedData sharedData;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;

        public EcsWorld World { get; set; }

        public RagdollSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                Process(entity, ref pedStash.Get(entity));
            }
        }

        private void Process(EcsEntity entity, ref ConvertedPed convertedPed) {
            Ped ped = convertedPed.thisPed;
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

            EnsurePermanentRagdoll(ref convertedPed);

            bool newHelperRequest = convertedPed.naturalMotionBuilder != null;
            bool ragdollRequested = convertedPed.ragdollRequest.time != 0 || newHelperRequest;
            if (!ragdollRequested) {
                return;
            }

#if DEBUG && DEBUG_EVERY_FRAME
            int time = convertedPed.ragdollRequest.time;
            sharedData.logger.WriteInfo($"Exec Ragdoll for {convertedPed.name} t:{time} newNm:{newHelperRequest}");
#endif
            ExecuteNewRagdoll(entity, ref convertedPed);
        }

        private void EnsurePermanentRagdoll(ref ConvertedPed convertedPed) {
            if (!convertedPed.permanentRagdoll) {
                return;
            }

            PedEffects.ResetRagdollTime(convertedPed.thisPed);
            if (PedEffects.IsPedGettingUp(convertedPed.thisPed)) {
#if DEBUG && DEBUG_EVERY_FRAME
                sharedData.logger.WriteInfo($"Force permanent ragdoll to {convertedPed.name}");
#endif
                convertedPed.ragdollRequest = (PERMANENT_RAGDOLL_TIME, RagdollType.Relax);
            }
        }

        private void ExecuteNewRagdoll(EcsEntity entity, ref ConvertedPed convertedPed) {
            bool ragdollStarted = TryPlayNaturalMotion(entity, ref convertedPed);
            if (!ragdollStarted) {
                (int time, RagdollType type) = convertedPed.ragdollRequest;
                convertedPed.thisPed.Ragdoll(time, type);
            }

            convertedPed.RemoveRagdollRequest();
        }

        private bool TryPlayNaturalMotion(EcsEntity entity, ref ConvertedPed convertedPed) {
            if (convertedPed.naturalMotionBuilder == null) {
                return false;
            }

            CustomHelper newHelper = convertedPed.naturalMotionBuilder(sharedData, entity, convertedPed.thisPed);
            convertedPed.naturalMotionBuilder = null;
            convertedPed.nmBuilderOverrideForbidden = false;

#if DEBUG
            sharedData.logger.WriteInfo($"Starting NM helper: {newHelper.GetType().Name}");
#endif

            StopAllNMBehaviours(convertedPed.thisPed);
            convertedPed.activeNmHelper = newHelper;
            convertedPed.activeNmHelper.Start(convertedPed.ragdollRequest.time);
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
                StopAllNMBehaviours(convertedPed.thisPed);
                convertedPed.thisPed.CancelRagdoll();
            }

            convertedPed.ragdollReset = false;
            convertedPed.permanentRagdoll = false;
            convertedPed.ragdollRequest = default;
            convertedPed.activeNmHelper = null;
        }

        private static void StopAllNMBehaviours(Ped ped) {
            ped.Euphoria.StopAllBehaviors.Start();
        }

        public void Dispose() {
            foreach (EcsEntity entity in peds) {
                ResetRagdoll(ref pedStash.Get(entity));
            }
        }
    }
}