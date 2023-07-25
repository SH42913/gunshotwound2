// namespace GunshotWound2.World {
//     using GTA;
//     using HitDetection;
//     using Leopotam.Ecs;
//     using Utils;
//
//     public sealed class RemoveWoundedPedSystem : IEcsRunSystem {
//         private readonly EcsFilter<WoundedPedComponent, IsNpcMarker> npcs = null;
//         private readonly SharedData sharedData = null;
//
//         public void Run() {
//             foreach (int index in npcs) {
//                 WoundedPedComponent woundedPed = npcs.Get1(index);
//                 Ped ped = woundedPed.ThisPed;
//                 if (ped.IsAlive && !OutOfRemoveRange(ped)) {
//                     continue;
//                 }
//
//                 sharedData.gswWorld.gswPeds.Remove(ped);
//                 woundedPed.ThisPed = null;
//                 npcs.GetEntity(index).Destroy();
//
// #if DEBUG
//                 ped.AttachedBlip.Delete();
// #endif
//             }
//         }
//
//         private bool OutOfRemoveRange(Ped ped) {
//             float removeRange = sharedData.mainConfig.NpcConfig.RemovePedRange;
//             return World.GetDistance(Game.Player.Character.Position, ped.Position) > removeRange;
//         }
//     }
// }