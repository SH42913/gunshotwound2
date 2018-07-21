using System;
using GTA;
using GunshotWound2.Components.NpcComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.NpcSystems
{
    [EcsInject]
    public class NpcSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<WoundedPedComponent, NpcComponent> _npcs;
        
        private int _ticks;
        private static readonly Random Random = new Random();
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(NpcSystem);
            
            var ticksToRefresh = _config.Data.TicksToRefresh;
            if((++_ticks + 2) % ticksToRefresh != 0) return;
            
            AddPeds();
            RemovePeds();
        }

        private void AddPeds()
        {
            float addRange = _config.Data.NpcConfig.AddingPedRange;
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

                NpcComponent npcComponent;
                WoundedPedComponent woundedPed;
                 _ecsWorld.CreateEntityWith(out npcComponent, out woundedPed);
                
                woundedPed.ThisPed = nearbyPed;

                var newHealth = Random.Next(
                    _config.Data.NpcConfig.MaximalHealth/2,
                    _config.Data.NpcConfig.MaximalHealth);
                woundedPed.Health = newHealth;
                woundedPed.Armor = nearbyPed.Armor;
                woundedPed.ThisPed.MaxHealth = newHealth;
                woundedPed.ThisPed.Health = newHealth;
                woundedPed.ThisPed.CanWrithe = false;

                woundedPed.StopBleedingAmount = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalBleedStopSpeed/2,
                    _config.Data.NpcConfig.MaximalBleedStopSpeed);
                woundedPed.DefaultAccuracy = nearbyPed.Accuracy;
                
                woundedPed.HeShe = nearbyPed.Gender == Gender.Male
                    ? "He"
                    : "She";
                woundedPed.HisHer = nearbyPed.Gender == Gender.Male
                    ? "His"
                    : "Her";
                
                woundedPed.PainMeter = 0;
                woundedPed.MaximalPain = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalPain/2,
                    _config.Data.NpcConfig.MaximalPain);
                woundedPed.PainRecoverSpeed = Random.NextFloat(
                    _config.Data.NpcConfig.MaximalPainRecoverSpeed/2,
                    _config.Data.NpcConfig.MaximalPainRecoverSpeed);

                addedPedCount++;

#if DEBUG
                nearbyPed.AddBlip();       
#endif
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

                _npcs.Components1[pedIndex].ThisPed = null;
                _ecsWorld.RemoveEntity(_npcs.Entities[pedIndex]);
#if DEBUG
                ped.CurrentBlip.Remove();
#endif
            }
        }

        private bool OutRemoveRange(Ped ped)
        {
            var removeRange = _config.Data.NpcConfig.RemovePedRange;
            return World.GetDistance(Game.Player.Character.Position, ped.Position) > removeRange;
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}