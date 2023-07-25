// using System;
// using GTA;
// using GTA.Native;
// using GunshotWound2.Configs;
// using GunshotWound2.Effects;
// using GunshotWound2.GUI;
// using GunshotWound2.HitDetection;
// using GunshotWound2.Pain;
// using GunshotWound2.World;
// using Leopotam.Ecs;
//
// namespace GunshotWound2.Player {
//     using Utils;
//
//     public sealed class PlayerSystem : IEcsInitSystem, IEcsRunSystem {
//         private readonly EcsWorld ecsWorld = null;
//         private readonly EcsFilter<WoundedPedComponent, IsPlayerMarker> players = null;
//         private readonly SharedData sharedData = null;
//
//         public void Init() {
//             if (sharedData.mainConfig.PlayerConfig.WoundedPlayerEnabled) {
//                 CreateGswPlayer(Game.Player.Character);
//             }
//         }
//
//         public void Run() {
//             var playerConfig = sharedData.mainConfig.PlayerConfig;
//             foreach (int index in players) {
//                 var woundedPed = players.Get1(index);
//                 var playerEntity = players.GetEntity(index);
//                 GTA.Player player = Game.Player;
//                 Ped playerPed = player.Character;
//                 if (!woundedPed.ThisPed.Position.Equals(playerPed.Position)) {
//                     SwitchGswPed(playerPed, woundedPed, playerEntity);
//                 }
//
//                 var pedHealth = woundedPed.PedHealth;
//                 if (pedHealth > playerConfig.MaximalHealth) {
//                     ecsWorld.ScheduleEventWithTarget<InstantHealEvent>(playerEntity);
//                 } else if (pedHealth < playerConfig.MinimalHealth && !woundedPed.IsDead) {
//                     woundedPed.Health = pedHealth;
//                     woundedPed.IsDead = true;
//                     player.WantedLevel = 0;
//                     player.IgnoredByEveryone = false;
//                     Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
//
//                     var ragdollEvent = ecsWorld.CreateEntityWith<SetPedToRagdollEvent>();
//                     ragdollEvent.RagdollState = RagdollStates.PERMANENT;
//                     ragdollEvent.Entity = playerEntity;
//
//                     ecsWorld.RemoveComponent<PainComponent>(players.Entities[index], true);
//                     ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = playerEntity;
//                 }
//             }
//         }
//
//         private void CreateGswPlayer(Ped ped) {
//             PlayerConfig playerConfig = sharedData.mainConfig.PlayerConfig;
//             EcsEntity entity = ecsWorld.NewEntity();
//             entity.SetMarker<IsPlayerMarker>();
//
//             ref WoundedPedComponent woundedPed = ref entity.Get<WoundedPedComponent>();
//             woundedPed.PainState = PainStates.DEADLY;
//
//             woundedPed.IsPlayer = true;
//             woundedPed.IsMale = ped.Gender == Gender.Male;
//             woundedPed.ThisPed = ped;
//             woundedPed.IsDead = false;
//
//             woundedPed.Armor = ped.Armor;
//             woundedPed.Health = playerConfig.MaximalHealth;
//             woundedPed.PedHealth = woundedPed.Health;
//             woundedPed.PedMaxHealth = woundedPed.Health;
//
//             woundedPed.MaximalPain = playerConfig.MaximalPain;
//             woundedPed.PainRecoverSpeed = playerConfig.PainRecoverSpeed;
//             woundedPed.StopBleedingAmount = playerConfig.BleedHealingSpeed;
//
//             woundedPed.Crits = 0;
//             woundedPed.BleedingCount = 0;
//             woundedPed.MostDangerBleedingEntity = null;
//
//             woundedPed.ThisPed.CanWrithe = false;
//             woundedPed.ThisPed.DiesOnLowHealth = false;
//             woundedPed.ThisPed.CanWearHelmet = true;
//             woundedPed.ThisPed.CanSufferCriticalHits = false;
//
//             int totalHealth = playerConfig.MaximalHealth - playerConfig.MinimalHealth;
//             float deadlyRate = totalHealth * playerConfig.BleedHealingSpeed;
//             sharedData.mainConfig.WoundConfig.EmergencyBleedingLevel = (float)Math.Sqrt(deadlyRate);
//
//             ecsWorld.ScheduleEventWithTarget<NoPainChangeStateEvent>(entity);
//             sharedData.gswWorld.gswPeds.Add(ped, entity);
//             sharedData.playerEntity = entity;
//         }
//
//         private void SwitchGswPed(Ped ped, WoundedPedComponent oldPed, EcsEntity oldEntity) {
//             var playerConfig = _config.Data.PlayerConfig;
//
//             oldPed.IsPlayer = false;
//             oldPed.Health -= playerConfig.MinimalHealth;
//             oldPed.PedHealth = oldPed.Health;
//             oldPed.PedMaxHealth = 100;
//
//             ecsWorld.RemoveComponent<IsPlayerMarker>(oldEntity);
//             ecsWorld.AddComponent<IsNpcMarker>(oldEntity);
// #if DEBUG
//             oldPed.ThisPed.AddBlip();
// #endif
//
//             if (_world.Data.GswPeds.TryGetValue(ped, out var newEntity)) {
//                 var newPed = ecsWorld.GetComponent<WoundedPedComponent>(newEntity);
//                 newPed.IsPlayer = true;
//                 newPed.IsDead = false;
//                 newPed.Health += playerConfig.MinimalHealth;
//                 newPed.PedHealth = newPed.Health;
//                 newPed.PedMaxHealth = playerConfig.MaximalHealth;
//
//                 newPed.MaximalPain = playerConfig.MaximalPain;
//                 newPed.PainRecoverSpeed = playerConfig.PainRecoverSpeed;
//                 newPed.StopBleedingAmount = playerConfig.BleedHealingSpeed;
//
//                 ecsWorld.RemoveComponent<IsNpcMarker>(newEntity);
//                 ecsWorld.AddComponent<IsPlayerMarker>(newEntity);
//                 UpdatePainState(newEntity, newPed.PainState);
//
//                 playerConfig.PlayerEntity = newEntity;
// #if DEBUG
//                 sharedData.logger.WriteInfo($"Switched to {newEntity}");
//                 newPed.ThisPed.AttachedBlip.Delete();
// #endif
//             } else {
//                 CreateGswPlayer(ped);
//             }
//         }
//
//         private void UpdatePainState(int entity, PainStates state) {
//             BaseChangePainStateEvent evt;
//
//             switch (state) {
//                 case PainStates.NONE:
//                     evt = ecsWorld.AddComponent<NoPainChangeStateEvent>(entity);
//                     break;
//                 case PainStates.MILD:
//                     evt = ecsWorld.AddComponent<MildPainChangeStateEvent>(entity);
//                     break;
//                 case PainStates.AVERAGE:
//                     evt = ecsWorld.AddComponent<AveragePainChangeStateEvent>(entity);
//                     break;
//                 case PainStates.INTENSE:
//                     evt = ecsWorld.AddComponent<IntensePainChangeStateEvent>(entity);
//                     break;
//                 case PainStates.UNBEARABLE:
//                     evt = ecsWorld.AddComponent<UnbearablePainChangeStateEvent>(entity);
//                     break;
//                 case PainStates.DEADLY:
//                     evt = ecsWorld.AddComponent<DeadlyPainChangeStateEvent>(entity);
//                     break;
//                 default: throw new ArgumentOutOfRangeException();
//             }
//
//             evt.ForceUpdate = true;
//             evt.Entity = entity;
//         }
//
//         private void SendMessage(string message, int pedEntity, NotifyLevels level = NotifyLevels.COMMON) {
// #if !DEBUG
//             if (level == NotifyLevels.DEBUG) return;
//             if (_config.Data.PlayerConfig.PlayerEntity != pedEntity) return;
// #endif
//
//             var notification = ecsWorld.CreateEntityWith<ShowNotificationEvent>();
//             notification.Level = level;
//             notification.StringToShow = message;
//         }
//     }
// }