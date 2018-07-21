using System;
using GTA.Native;
using GunshotWound2.Components;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class PainSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<PainComponent> _components;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(PainSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                int pedEntity = component.PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    var additionalPain = component.PainAmount;
                    
                    var painDeviation = Random.NextFloat(
                        -_config.Data.WoundConfig.PainDeviation, 
                        _config.Data.WoundConfig.PainDeviation);
                    woundedPed.PainMeter += _config.Data.WoundConfig.PainMultiplier * additionalPain + painDeviation;

                    if (woundedPed.IsPlayer)
                    {
                        if (additionalPain > 30)
                        {
                            Function.Call(Hash._SET_CAM_EFFECT, 1);
                        }
                        
                        if (additionalPain > 60)
                        {
                            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
                            _ecsWorld.CreateEntityWith<AdrenalineComponent>();
                        }
                    }
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }
    }
}