using GTA.Native;
using GunshotWound2.Components.Events.BodyHitEvents;
using GunshotWound2.Components.MarkComponents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems
{
    [EcsInject]
    public class HitCleanSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<WoundedPedComponent, HaveDamageMarkComponent> _peds;
        private EcsFilter<CheckBodyHitEvent> _requestsToClean;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(HitCleanSystem);
#endif
            
            for (int i = 0; i < _requestsToClean.EntitiesCount; i++)
            {
                int pedEntity = _requestsToClean.Components1[i].Entity;
                var ped = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity).ThisPed;

                if (ped != null)
                {
                    Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
                    Function.Call(Hash.CLEAR_PED_LAST_DAMAGE_BONE, ped);
                }
                
                _ecsWorld.RemoveEntity(_requestsToClean.Entities[i]);
            }

            for (int i = 0; i < _peds.EntitiesCount; i++)
            {
                _ecsWorld.RemoveComponent<HaveDamageMarkComponent>(_peds.Entities[i]);
            }
        }
    }
}