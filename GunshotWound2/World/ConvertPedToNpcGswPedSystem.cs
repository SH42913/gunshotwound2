// using GTA;
// using GTA.Native;
// using GunshotWound2.Configs;
// using GunshotWound2.HitDetection;
// using GunshotWound2.Pain;
// using GunshotWound2.Utils;
// using Leopotam.Ecs;
//
// namespace GunshotWound2.World {
//     using System;
//
//     public sealed class ConvertPedToNpcGswPedSystem : IEcsRunSystem {
//         private readonly EcsWorld ecsWorld = null;
//         private readonly SharedData sharedData = null;
//
//         public void Run() {
//             Random random = sharedData.random;
//             GswWorld gswWorld = sharedData.gswWorld;
//             NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
//
//             while (gswWorld.pedsToConvert.Count > 0) {
//                 Ped pedToConvert = gswWorld.pedsToConvert.Dequeue();
//
//                 EcsEntity entity = ecsWorld.NewEntity();
//                 ref WoundedPedComponent woundedPed = ref entity.Get<WoundedPedComponent>();
//                 entity.SetMarker<IsNpcMarker>();
//
//                 woundedPed.ThisPed = pedToConvert;
//                 woundedPed.IsMale = pedToConvert.Gender == Gender.Male;
//                 woundedPed.IsDead = false;
//                 woundedPed.IsPlayer = false;
//                 woundedPed.Armor = pedToConvert.Armor;
//                 woundedPed.Health = random.Next(npcConfig.MinStartHealth, npcConfig.MaxStartHealth);
//                 woundedPed.PedHealth = woundedPed.Health;
//                 woundedPed.PedMaxHealth = woundedPed.Health;
//
//                 woundedPed.ThisPed.CanWrithe = false;
//                 woundedPed.ThisPed.CanWearHelmet = true;
//                 woundedPed.ThisPed.DiesOnLowHealth = false;
//                 woundedPed.ThisPed.CanSufferCriticalHits = true;
//                 woundedPed.ThisPed.CanSwitchWeapons = true;
//                 woundedPed.ThisPed.CanBeShotInVehicle = true;
//
//                 // if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, pedToConvert)) {
//                 //     //TODO Why is it here?
//                 // }
//
//                 woundedPed.StopBleedingAmount = random.NextFloat(
//                     npcConfig.MaximalBleedStopSpeed / 2,
//                     npcConfig.MaximalBleedStopSpeed);
//
//                 if (npcConfig.MinAccuracy > 0 && npcConfig.MaxAccuracy > 0) {
//                     pedToConvert.Accuracy = random.Next(npcConfig.MinAccuracy, npcConfig.MaxAccuracy + 1);
//                 }
//
//                 if (npcConfig.MinShootRate > 0 && npcConfig.MaxShootRate > 0) {
//                     pedToConvert.ShootRate = random.Next(npcConfig.MinShootRate, npcConfig.MaxShootRate);
//                 }
//
//                 woundedPed.DefaultAccuracy = pedToConvert.Accuracy;
//
//                 woundedPed.MaximalPain = random.NextFloat(npcConfig.LowerMaximalPain, npcConfig.UpperMaximalPain);
//
//                 woundedPed.PainRecoverSpeed = random.NextFloat(
//                     npcConfig.MaximalPainRecoverSpeed / 2,
//                     npcConfig.MaximalPainRecoverSpeed);
//
//                 woundedPed.Crits = 0;
//                 woundedPed.BleedingCount = 0;
//                 woundedPed.MostDangerBleedingEntity = null;
//
//                 ecsWorld.ScheduleEventWithTarget<NoPainChangeStateEvent>(entity);
//
//                 gswWorld.gswPeds.Add(pedToConvert, entity);
//
// #if DEBUG
//                 pedToConvert.AddBlip();
// #endif
//             }
//         }
//     }
// }