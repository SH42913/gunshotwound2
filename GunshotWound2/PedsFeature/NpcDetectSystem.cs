namespace GunshotWound2.PedsFeature {
    using System;
    using Configs;
    using GTA;
    using GTA.Native;
    using Scellecs.Morpeh;
    using Services;

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
            PedsConfig pedsConfig = sharedData.mainConfig.pedsConfig;
            float addRange = pedsConfig.AddingPedRange;
            if (addRange <= PedsConfig.MINIMAL_RANGE_FOR_WOUNDED_PEDS) {
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
                worldService.forceRefreshRequest = false;
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

            if (ped.IsPlayer || !ped.IsHuman || ped.IsDead || PedEffects.IsPedInWrithe(ped)) {
                return false;
            }

            if (!sharedData.mainConfig.pedsConfig.ScanOnlyDamaged) {
                return true;
            }

            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, ped);
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
    }
}