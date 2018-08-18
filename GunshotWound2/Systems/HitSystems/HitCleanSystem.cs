using GTA.Native;
using GunshotWound2.Components.Events.BodyHitEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems
{
    [EcsInject]
    public class HitCleanSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<CheckBodyHitEvent> _requestsToClean;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(HitCleanSystem);
            
            for (int i = 0; i < _requestsToClean.EntitiesCount; i++)
            {
                int pedEntity = _requestsToClean.Components1[i].PedEntity;
                var ped = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity).ThisPed;
                
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
                Function.Call(Hash.CLEAR_PED_LAST_DAMAGE_BONE, ped);
                
                _ecsWorld.RemoveEntity(_requestsToClean.Entities[i]);
            }
        }
    }
}