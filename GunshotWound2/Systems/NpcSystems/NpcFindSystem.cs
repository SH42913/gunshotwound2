using System.Collections.Generic;
using GTA;
using GunshotWound2.Components.Events.NpcEvents;
using GunshotWound2.Components.MarkComponents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.NpcSystems
{
    [EcsInject]
    public class NpcFindSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<WoundedPedComponent, NpcMarkComponent> _npcs;

        private int _ticks;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(NpcFindSystem);
#endif
            if(++_ticks % _config.Data.NpcConfig.TickToUpdate != 0) return;
            _ticks = 0;
            
            FindPeds();
        }

        private void FindPeds()
        {
            float addRange = _config.Data.NpcConfig.AddingPedRange;
            if(addRange < 1) return;
            
            Ped[] allPeds = World.GetNearbyPeds(Game.Player.Character, addRange);
            List<Ped> pedsToAdd = new List<Ped>();
            
            for (int nearPed = 0; nearPed < allPeds.Length; nearPed++)
            {
                Ped nearbyPed = allPeds[nearPed];
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
                
                pedsToAdd.Add(nearbyPed);
            }

            if(pedsToAdd.Count <= 0) return;
            _ecsWorld.CreateEntityWith<ConvertPedsToWoundedPedsEvent>().PedsInRange = pedsToAdd.ToArray();
        }
    }
}