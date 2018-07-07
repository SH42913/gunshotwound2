using GTA;
using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems
{
    [EcsInject]
    public class BleedingSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<WoundConfig> _config;
        private EcsFilter<BleedingComponent> _components;
        
        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(BleedingSystem);
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed == null || component.BleedSeverity < 0)
                {
                    _ecsWorld.RemoveEntity(_components.Entities[i]);
                    continue;
                }
                
                woundedPed.Health -= component.BleedSeverity * Game.LastFrameTime;
                component.BleedSeverity -= _config.Data.StopBleedingAmount * Game.LastFrameTime;
                woundedPed.ThisPed.Health = (int) woundedPed.Health;
            }
        }
    }
}