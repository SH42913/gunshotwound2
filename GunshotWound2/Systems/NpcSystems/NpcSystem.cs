using System;
using GTA;
using GunshotWound2.Components.NpcComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.NpcSystems
{
    [EcsInject]
    public class NpcSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        private EcsFilterSingle<NpcConfig> _config;
        private EcsFilter<WoundedPedComponent, NpcComponent> _npcs;
        private int ticks;
        private static readonly Random _random = new Random();
        
        public void Run()
        {
            var ticksToRefresh = _mainConfig.Data.TicksToRefresh;
            if(++ticks % ticksToRefresh != 0) return;
            
            AddPeds();
            RemovePeds();
        }

        private void AddPeds()
        {
            float addRange = _config.Data.AddingPedRange;
            if(addRange < 1) return;
            
            int addedPedCount = 0;
            Ped[] nearbyPeds = World.GetNearbyPeds(Game.Player.Character, addRange);
            for (int nearPed = 0; nearPed < nearbyPeds.Length; nearPed++)
            {
                Ped nearbyPed = nearbyPeds[nearPed];
                if(nearbyPed.IsDead || !nearbyPed.IsHuman) continue;

                bool componentExist = false;
                for (int pedIndex = 0; pedIndex < _npcs.EntitiesCount; pedIndex++)
                {
                    var ped = _npcs.Components1[pedIndex].ThisPed;
                    if (!ped.Position.Equals(nearbyPed.Position)) continue;

                    componentExist = true;
                    break;
                }
                if(componentExist) continue;

                var entity = _ecsWorld.CreateEntity();
                _ecsWorld.AddComponent<NpcComponent>(entity);
                
                var woundPed = _ecsWorld.AddComponent<WoundedPedComponent>(entity);
                woundPed.ThisPed = nearbyPed;
                woundPed.Armor = nearbyPed.Armor;
                woundPed.IsMale = nearbyPed.Gender == Gender.Male;
                woundPed.Health = _random.Next(30, 100);
                woundPed.MaximalPain = _random.Next(50, 100);

                nearbyPed.CanWrithe = false;
                addedPedCount++;
                
                if(!_mainConfig.Data.Debug) continue;
                nearbyPed.AddBlip();
            }

            if (addedPedCount > 1)
            {
                SendDebug($"+ {addedPedCount} peds");
            }
        }

        private void RemovePeds()
        {
            for (int pedIndex = 0; pedIndex < _npcs.EntitiesCount; pedIndex++)
            {
                var ped = _npcs.Components1[pedIndex].ThisPed;
                if(ped.IsAlive && !OutRemoveRange(ped)) continue;
                
                _ecsWorld.RemoveEntity(_npcs.Entities[pedIndex]);
                ped.CurrentBlip.Remove();
            }
        }

        private bool OutRemoveRange(Ped ped)
        {
            var removeRange = _config.Data.RemovePedRange;
            return World.GetDistance(Game.Player.Character.Position, ped.Position) > removeRange;
        }

        private void SendDebug(string message)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
        }
    }
}