using System;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class BleedingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<BleedingComponent> _bleedings;
        private DateTime _lastUpdateTime;

        public void Initialize()
        {
            _lastUpdateTime = DateTime.Now;
        }

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BleedingSystem);
#endif
            
            ProcessBleedings();
        }

        private void ProcessBleedings()
        {
            var timeBetweenFrames = DateTime.Now - _lastUpdateTime;
            _lastUpdateTime = DateTime.Now;
            var frameTimeInSeconds = (float) timeBetweenFrames.TotalSeconds;
            
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
                
                woundedPed.Health -= component.BleedSeverity * frameTimeInSeconds;
                component.BleedSeverity -= woundedPed.StopBleedingAmount * frameTimeInSeconds;
                woundedPed.ThisPed.Health = (int) woundedPed.Health;
            }
        }

        public void Destroy()
        {
            
        }
    }
}