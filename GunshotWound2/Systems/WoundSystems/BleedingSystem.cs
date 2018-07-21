using GTA;
using GunshotWound2.Components.WoundComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class BleedingSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<BleedingComponent> _components;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(BleedingSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null || component.BleedSeverity <= 0f)
                {
                    _ecsWorld.RemoveEntity(_components.Entities[i]);
                    continue;
                }
                
                if(woundedPed.IsDead) continue;
                
                woundedPed.Health -= component.BleedSeverity * Game.LastFrameTime;
                component.BleedSeverity -= woundedPed.StopBleedingAmount * Game.LastFrameTime;
                woundedPed.ThisPed.Health = (int) woundedPed.Health;
            }
        }
    }
}