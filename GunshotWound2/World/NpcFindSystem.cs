// namespace GunshotWound2.World {
//     using Configs;
//     using GTA;
//     using GTA.Native;
//     using Leopotam.Ecs;
//     using Utils;
//
//     public sealed class NpcFindSystem : IEcsRunSystem {
//         private const int MAX_TIME_TO_FIND_IN_MS = 10;
//
//         private readonly SharedData sharedData = null;
//
//         private Ped[] pedsToProcess;
//         private int lastCheckedPedIndex;
//
//         public void Run() {
//             NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
//             float addRange = npcConfig.AddingPedRange;
//             if (addRange <= GunshotWound2.MINIMAL_RANGE_FOR_WOUNDED_PEDS) {
//                 return;
//             }
//
//             sharedData.stopwatch.Restart();
//             Ped playerPed = Game.Player.Character;
//             bool refreshPeds = sharedData.gswWorld.forceUpdateRequest
//                                || pedsToProcess == null
//                                || lastCheckedPedIndex + 1 >= pedsToProcess.Length - 1;
//
//             if (refreshPeds) {
//                 pedsToProcess = World.GetNearbyPeds(playerPed, addRange);
//                 lastCheckedPedIndex = 0;
//             }
//
//             for (int index = lastCheckedPedIndex; index < pedsToProcess.Length; index++) {
//                 if (sharedData.stopwatch.ElapsedMilliseconds > MAX_TIME_TO_FIND_IN_MS) {
//                     break;
//                 }
//
//                 lastCheckedPedIndex = index;
//                 Ped pedToCheck = pedsToProcess[index];
//                 if (!pedToCheck.IsHuman || pedToCheck.IsDead || pedToCheck.IsPlayer) {
//                     continue;
//                 }
//
//                 if (!PedInTargetList(playerPed, pedToCheck)) {
//                     continue;
//                 }
//
//                 if (npcConfig.ScanOnlyDamaged && !Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, pedToCheck)) {
//                     continue;
//                 }
//
//                 if (!sharedData.gswWorld.gswPeds.ContainsKey(pedToCheck)) {
//                     sharedData.gswWorld.pedsToConvert.Enqueue(pedToCheck);
//                 }
//             }
//
//             sharedData.stopwatch.Stop();
//         }
//
//         private bool PedInTargetList(Ped playerPed, Ped pedToCheck) {
//             GswTargets targets = sharedData.mainConfig.NpcConfig.Targets;
//             if (HasFlag(targets, GswTargets.ALL)) {
//                 return true;
//             }
//
//             Relationship relationship = playerPed.GetRelationshipWithPed(pedToCheck);
//             if (!HasFlag(targets, GswTargets.PEDESTRIAN) && relationship == Relationship.Pedestrians) {
//                 return false;
//             }
//
//             if (!HasFlag(targets, GswTargets.COMPANION) && relationship == Relationship.Companion) {
//                 return false;
//             }
//
//             if (!HasFlag(targets, GswTargets.NEUTRAL) && relationship == Relationship.Neutral) {
//                 return false;
//             }
//
//             if (!HasFlag(targets, GswTargets.DISLIKE) && relationship == Relationship.Dislike) {
//                 return false;
//             }
//
//             if (!HasFlag(targets, GswTargets.HATE) && relationship == Relationship.Hate) {
//                 return false;
//             }
//
//             if (!HasFlag(targets, GswTargets.LIKE) && relationship == Relationship.Like) {
//                 return false;
//             }
//
//             if (!HasFlag(targets, GswTargets.RESPECT) && relationship == Relationship.Respect) {
//                 return false;
//             }
//
//             return true;
//         }
//
//         private static bool HasFlag(GswTargets value, GswTargets flag) {
//             var targetInt = (int)value;
//             var flagInt = (int)flag;
//             return (targetInt & flagInt) == flagInt;
//         }
//     }
// }