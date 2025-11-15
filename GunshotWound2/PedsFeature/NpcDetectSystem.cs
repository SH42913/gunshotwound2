namespace GunshotWound2.PedsFeature {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Configs;
    using GTA;
    using GTA.Math;
    using Scellecs.Morpeh;
    using Services;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class NpcDetectSystem : ISystem {
        private const int MAX_TIME_TO_FIND_IN_MS = 1;

        private readonly SharedData sharedData;
        private readonly DistanceToPlayerComparer pedComparer;
        private readonly Stopwatch stopwatch;

        private Ped[] npcToProcess;
        private int lastCheckedPedIndex;

        public EcsWorld World { get; set; }

        public NpcDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            pedComparer = new DistanceToPlayerComparer();
            stopwatch = Stopwatch.StartNew();
        }

        void IInitializer.OnAwake() { }
        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            stopwatch.Restart();
            Ped playerPed = Game.Player.Character;
            WorldService worldService = sharedData.worldService;
            bool refreshPeds = worldService.forceRefreshRequest
                               || npcToProcess == null
                               || lastCheckedPedIndex + 1 >= npcToProcess.Length - 1;

            if (refreshPeds) {
                npcToProcess = GTA.World.GetAllPeds();
                pedComparer.playerPosition = playerPed.Position;
                Array.Sort(npcToProcess, pedComparer);

                lastCheckedPedIndex = 0;
                worldService.forceRefreshRequest = false;
            }

            for (int index = lastCheckedPedIndex; index < npcToProcess.Length; index++) {
                if (stopwatch.ElapsedMilliseconds > MAX_TIME_TO_FIND_IN_MS) {
                    break;
                }

                lastCheckedPedIndex = index;
                Ped pedToCheck = npcToProcess[index];
                if (pedToCheck == playerPed) {
                    continue;
                }

                if (IsReadyToConvert(pedToCheck) && IsInTargetList(pedToCheck, playerPed)) {
                    worldService.EnqueueToConvert(pedToCheck);
                }
            }

            stopwatch.Stop();
        }

        private bool IsReadyToConvert(Ped ped) {
            return !sharedData.worldService.IsConverted(ped)
                   && !ped.IsPlayer
                   && ped.IsHuman
                   && !ped.IsDead
                   && !PedEffects.IsPedInWrithe(ped);
        }

        private bool IsInTargetList(Ped pedToCheck, Ped playerPed) {
            GswTargets targets = sharedData.mainConfig.pedsConfig.Targets;
            if (targets == GswTargets.ALL) {
                return true;
            }

            Relationship relationship = playerPed.GetRelationshipWithPed(pedToCheck);
            switch (relationship) {
                case Relationship.Companion:   return HasFlag(targets, GswTargets.COMPANION);
                case Relationship.Respect:     return HasFlag(targets, GswTargets.RESPECT);
                case Relationship.Like:        return HasFlag(targets, GswTargets.LIKE);
                case Relationship.Neutral:     return HasFlag(targets, GswTargets.NEUTRAL);
                case Relationship.Dislike:     return HasFlag(targets, GswTargets.DISLIKE);
                case Relationship.Hate:        return HasFlag(targets, GswTargets.HATE);
                case Relationship.Pedestrians: return HasFlag(targets, GswTargets.PEDESTRIAN);
                case Relationship.Dead:        return false;
                default:                       throw new ArgumentOutOfRangeException();
            }
        }

        private static bool HasFlag(GswTargets value, GswTargets flag) {
            var targetInt = (int)value;
            var flagInt = (int)flag;
            return (targetInt & flagInt) == flagInt;
        }

        private sealed class DistanceToPlayerComparer : IComparer<Ped> {
            public Vector3 playerPosition;

            public int Compare(Ped x, Ped y) {
                float xDist = GTA.World.GetDistance(x!.Position, playerPosition);
                float yDist = GTA.World.GetDistance(y!.Position, playerPosition);
                return xDist.CompareTo(yDist);
            }
        }
    }
}