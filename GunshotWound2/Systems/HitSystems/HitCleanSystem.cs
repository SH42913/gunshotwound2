using GTA.Native;
using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.WoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems
{
    [EcsInject]
    public class HitCleanSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<BodyDamageRequestComponent> _componentsToClean;
        
        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(HitCleanSystem);
            for (int i = 0; i < _componentsToClean.EntitiesCount; i++)
            {
                int pedEntity = _componentsToClean.Components1[i].PedEntity;
                var ped = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity).ThisPed;
                
                Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, ped);
                Function.Call(Hash.CLEAR_PED_LAST_DAMAGE_BONE, ped);
                _ecsWorld.RemoveEntity(_componentsToClean.Entities[i]);
            }
        }
    }
}