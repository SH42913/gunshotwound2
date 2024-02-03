namespace GunshotWound2.PedsFeature {
    using System;
    using Configs;
    using GTA;
    using GTA.Native;
    using Scellecs.Morpeh;

    public sealed class NpcDetectSystem : ISystem {
        private const int MAX_TIME_TO_FIND_IN_MS = 1;

        private readonly SharedData sharedData;

        private Ped[] npcToProcess;
        private int lastCheckedPedIndex;

        public Scellecs.Morpeh.World World { get; set; }

        public NpcDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        void IInitializer.OnAwake() { }
        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            float addRange = npcConfig.AddingPedRange;
            if (addRange <= MainConfig.MINIMAL_RANGE_FOR_WOUNDED_PEDS) {
                return;
            }

            sharedData.stopwatch.Restart();
            Ped playerPed = Game.Player.Character;
            WorldService worldService = sharedData.worldService;
            bool refreshPeds = worldService.forceRefreshRequest
                               || npcToProcess == null
                               || lastCheckedPedIndex + 1 >= npcToProcess.Length - 1;

            if (refreshPeds) {
                npcToProcess = GTA.World.GetNearbyPeds(playerPed, addRange);
                lastCheckedPedIndex = 0;
            }

            for (int index = lastCheckedPedIndex; index < npcToProcess.Length; index++) {
                if (sharedData.stopwatch.ElapsedMilliseconds > MAX_TIME_TO_FIND_IN_MS) {
                    break;
                }

                lastCheckedPedIndex = index;
                Ped pedToCheck = npcToProcess[index];
                if (IsReadyToConvert(pedToCheck) && IsInTargetList(pedToCheck, playerPed)) {
                    worldService.EnqueueToConvert(pedToCheck);
                }
            }

            sharedData.stopwatch.Stop();
        }

        private bool IsReadyToConvert(Ped ped) {
            if (sharedData.worldService.IsConverted(ped)) {
                return false;
            }

            if (ped.IsPlayer || !ped.IsHuman || ped.IsDead) {
                return false;
            }

            if (!sharedData.mainConfig.NpcConfig.ScanOnlyDamaged) {
                return true;
            }

            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, ped);
        }

        private bool IsInTargetList(Ped pedToCheck, Ped playerPed) {
            GswTargets targets = sharedData.mainConfig.NpcConfig.Targets;
            if (targets == GswTargets.ALL) {
                return true;
            }

            Relationship relationship = playerPed.GetRelationshipWithPed(pedToCheck);
            if (!HasFlag(targets, GswTargets.PEDESTRIAN) && relationship == Relationship.Pedestrians) {
                return false;
            }

            if (!HasFlag(targets, GswTargets.COMPANION) && relationship == Relationship.Companion) {
                return false;
            }

            if (!HasFlag(targets, GswTargets.NEUTRAL) && relationship == Relationship.Neutral) {
                return false;
            }

            if (!HasFlag(targets, GswTargets.DISLIKE) && relationship == Relationship.Dislike) {
                return false;
            }

            if (!HasFlag(targets, GswTargets.HATE) && relationship == Relationship.Hate) {
                return false;
            }

            if (!HasFlag(targets, GswTargets.LIKE) && relationship == Relationship.Like) {
                return false;
            }

            if (!HasFlag(targets, GswTargets.RESPECT) && relationship == Relationship.Respect) {
                return false;
            }

            return true;
        }

        private static bool HasFlag(GswTargets value, GswTargets flag) {
            var targetInt = (int)value;
            var flagInt = (int)flag;
            return (targetInt & flagInt) == flagInt;
        }
    }
}