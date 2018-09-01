using System.Collections.Generic;
using System.Diagnostics;
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
        private Stopwatch _stopwatch = new Stopwatch();
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(NpcFindSystem);
#endif
            
            FindPeds();
        }

        private void FindPeds()
        {
            float addRange = _config.Data.NpcConfig.AddingPedRange;
            if(addRange <= GunshotWound2.MINIMAL_RANGE_FOR_WOUNDED_PEDS) return;

            if (CheckNeedToUpdateWorldPeds())
            {
                _config.Data.NpcConfig.WorldPeds = World.GetNearbyPeds(Game.Player.Character, addRange);
                _config.Data.NpcConfig.LastCheckedPedIndex = 0;
            }

            Ped[] allPeds = _config.Data.NpcConfig.WorldPeds;
            List<Ped> pedsToAdd = new List<Ped>();
            
            _stopwatch.Restart();
            for (int worldPedIndex = _config.Data.NpcConfig.LastCheckedPedIndex; worldPedIndex < allPeds.Length; worldPedIndex++)
            {
                if(_stopwatch.ElapsedMilliseconds > _config.Data.NpcConfig.UpperBoundForFindInMs) break;
                
                _config.Data.NpcConfig.LastCheckedPedIndex = worldPedIndex;
                Ped pedToCheck = allPeds[worldPedIndex];
                
                if(pedToCheck.IsDead || !pedToCheck.IsHuman) continue;
                if(CheckWoundedPedExist(pedToCheck)) continue;
                
                pedsToAdd.Add(pedToCheck);
            }
            _stopwatch.Stop();

            if(pedsToAdd.Count <= 0) return;
            _ecsWorld.CreateEntityWith<ConvertPedToWoundedPedEvent>().PedsInRange = pedsToAdd.ToArray();
        }

        private bool CheckWoundedPedExist(Ped pedToCheck)
        {
            bool woundedPedExist = false;
            for (int pedIndex = 0; pedIndex < _npcs.EntitiesCount; pedIndex++)
            {
                var ped = _npcs.Components1[pedIndex].ThisPed;
                if (!ped.Position.Equals(pedToCheck.Position)) continue;

                woundedPedExist = true;
                break;
            }

            return woundedPedExist;
        }

        private bool CheckNeedToUpdateWorldPeds()
        {
            return _config.Data.NpcConfig.WorldPeds == null ||
                   _config.Data.NpcConfig.LastCheckedPedIndex == _config.Data.NpcConfig.WorldPeds.Length - 1;
        }
    }
}