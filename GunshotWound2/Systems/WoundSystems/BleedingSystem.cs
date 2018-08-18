using GTA;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class BleedingSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<BleedingComponent> _bleedings;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(BleedingSystem);
            
            ProcessBleedings();
        }

        private void ProcessBleedings()
        {
            for (int i = 0; i < _bleedings.EntitiesCount; i++)
            {
                var component = _bleedings.Components1[i];
                int pedEntity = _bleedings.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null || component.BleedSeverity <= 0f)
                {
                    _ecsWorld.RemoveEntity(_bleedings.Entities[i]);
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