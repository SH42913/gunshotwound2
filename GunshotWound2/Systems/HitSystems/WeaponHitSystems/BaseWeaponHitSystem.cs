using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.BodyHitEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public abstract class BaseWeaponHitSystem : IEcsInitSystem, IEcsRunSystem
    {
        protected EcsWorld EcsWorld;
        protected EcsFilter<WoundedPedComponent> Peds;
        protected uint[] WeaponHashes;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BaseWeaponHitSystem);
#endif
            
            for (int i = 0; i < Peds.EntitiesCount; i++)
            {
                if(!PedWasDamaged(WeaponHashes, Peds.Components1[i].ThisPed)) continue;

                int pedEntity = Peds.Entities[i];
                EcsWorld
                    .CreateEntityWith<CheckBodyHitEvent>()
                    .Entity = pedEntity;
                CreateComponent(pedEntity);
            }
        }

        protected abstract uint[] GetWeaponHashes();

        protected abstract void CreateComponent(int pedEntity);
        
        private bool PedWasDamaged(uint[] hashes, Ped target)
        {
            foreach (var hash in hashes)
            {
                if (!Function.Call<bool>(Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, target, hash, 0)) continue;
                
                return true;
            }

            return false;
        }

        public virtual void Initialize()
        {
            WeaponHashes = GetWeaponHashes();
        }

        public void Destroy()
        {
            
        }
    }
}