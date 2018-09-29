using System.Collections.Generic;
using System.Diagnostics;
using GTA;
using GTA.Native;
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
        private EcsFilter<WoundedPedComponent, NpcMarkComponent> _npcs;
        private EcsFilter<ForceWorldPedUpdateEvent> _forceUpdates;
        
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilterSingle<GswWorld> _world;
        
        private readonly Stopwatch _stopwatch = new Stopwatch();
        
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
            Ped playerPed = Game.Player.Character;

            if (CheckNeedToUpdateWorldPeds())
            {
                _config.Data.NpcConfig.WorldPeds = World.GetNearbyPeds(playerPed, addRange);
                _config.Data.NpcConfig.LastCheckedPedIndex = 0;
                _forceUpdates.RemoveAllEntities();
            }

            Ped[] allPeds = _config.Data.NpcConfig.WorldPeds;
            var pedsToAdd = new Queue<Ped>();
            
            _stopwatch.Restart();
            for (int worldPedIndex = _config.Data.NpcConfig.LastCheckedPedIndex; worldPedIndex < allPeds.Length; worldPedIndex++)
            {
                if(_stopwatch.ElapsedMilliseconds > _config.Data.NpcConfig.UpperBoundForFindInMs) break;
                
                _config.Data.NpcConfig.LastCheckedPedIndex = worldPedIndex;
                Ped pedToCheck = allPeds[worldPedIndex];
                
                if(!pedToCheck.IsHuman || pedToCheck.IsDead || pedToCheck.IsPlayer) continue;
                if(!PedInTargetList(playerPed, pedToCheck)) continue;
                if (_config.Data.NpcConfig.ScanOnlyDamaged && !Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, pedToCheck)) continue;
                if(CheckWoundedPedExist(pedToCheck)) continue;
                
                pedsToAdd.Enqueue(pedToCheck);
            }
            _stopwatch.Stop();

            if(pedsToAdd.Count <= 0) return;
            _ecsWorld.CreateEntityWith<ConvertPedToNpcGswPedEvent>().PedsToAdd = pedsToAdd;
        }

        private bool PedInTargetList(Ped playerPed, Ped pedToCheck)
        {
            var targets = _config.Data.NpcConfig.Targets;
            if (targets.HasFlag(GswTargets.ALL)) return true;
            
            var pedToPlayerRelationship = pedToCheck.GetRelationshipWithPed(playerPed);
            var playerToPedRelationship = playerPed.GetRelationshipWithPed(pedToCheck);
            
            if (!targets.HasFlag(GswTargets.PEDESTRIAN))
            {
                if(pedToPlayerRelationship == Relationship.Pedestrians || playerToPedRelationship == Relationship.Pedestrians) return false;
            }
            
            if (!targets.HasFlag(GswTargets.COMPANION))
            {
                if(pedToPlayerRelationship == Relationship.Companion || playerToPedRelationship == Relationship.Companion) return false;
            }
            
            if (!targets.HasFlag(GswTargets.NEUTRAL))
            {
                if(pedToPlayerRelationship == Relationship.Neutral || playerToPedRelationship == Relationship.Neutral) return false;
            }
            
            if (!targets.HasFlag(GswTargets.DISLIKE))
            {
                if(pedToPlayerRelationship == Relationship.Dislike || playerToPedRelationship == Relationship.Dislike) return false;
            }
            
            if (!targets.HasFlag(GswTargets.HATE))
            {
                if(pedToPlayerRelationship == Relationship.Hate || playerToPedRelationship == Relationship.Hate) return false;
            }
            
            if (!targets.HasFlag(GswTargets.LIKE))
            {
                if(pedToPlayerRelationship == Relationship.Like || playerToPedRelationship == Relationship.Like) return false;
            }
            
            if (!targets.HasFlag(GswTargets.RESPECT))
            {
                if(pedToPlayerRelationship == Relationship.Respect || playerToPedRelationship == Relationship.Respect) return false;
            }

            return true;
        }

        private bool CheckWoundedPedExist(Ped pedToCheck)
        {
            return _world.Data.GswPeds.ContainsKey(pedToCheck);
        }

        private bool CheckNeedToUpdateWorldPeds()
        {
            return _forceUpdates.EntitiesCount > 0 || _config.Data.NpcConfig.WorldPeds == null ||
                   _config.Data.NpcConfig.LastCheckedPedIndex + 1 >= _config.Data.NpcConfig.WorldPeds.Length - 1;
        }
    }
}