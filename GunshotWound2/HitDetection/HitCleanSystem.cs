using GTA.Native;
using GunshotWound2.Damage;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection
{
    [EcsInject]
    public sealed class HitCleanSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<WoundedPedComponent, HaveDamageMarkComponent> _peds = null;
        private readonly EcsFilter<CheckBodyHitEvent> _requestsToClean = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(HitCleanSystem);
#endif

            for (var i = 0; i < _requestsToClean.EntitiesCount; i++)
            {
                var pedEntity = _requestsToClean.Components1[i].Entity;
                var ped = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity).ThisPed;

                if (ped != null)
                {
                    ped.ClearLastWeaponDamage();
                    Function.Call(Hash.CLEAR_PED_LAST_DAMAGE_BONE, ped);
                }

                _ecsWorld.RemoveEntity(_requestsToClean.Entities[i]);
            }

            for (var i = 0; i < _peds.EntitiesCount; i++)
            {
                _ecsWorld.RemoveComponent<HaveDamageMarkComponent>(_peds.Entities[i]);
            }
        }
    }
}