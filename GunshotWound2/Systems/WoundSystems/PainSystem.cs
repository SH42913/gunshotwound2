using System;
using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class PainSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;
        private EcsFilter<AddPainEvent> _components;
        
        private static readonly Random Random = new Random();
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(PainSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var component = _components.Components1[i];
                int pedEntity = component.PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null && component.PainAmount > 0f)
                {
                    var additionalPain = component.PainAmount;
                    
                    var painDeviation = Random.NextFloat(
                        -_config.Data.WoundConfig.PainDeviation, 
                        _config.Data.WoundConfig.PainDeviation);
                    woundedPed.PainMeter += _config.Data.WoundConfig.PainMultiplier * additionalPain + painDeviation;
                    
                    if (additionalPain > _config.Data.WoundConfig.PainfulWoundValue/2)
                    {
                        if (woundedPed.IsPlayer)
                        {
                            Function.Call(Hash._SET_CAM_EFFECT, 1);
                        }
                    }
                        
                    if (additionalPain > _config.Data.WoundConfig.PainfulWoundValue)
                    {
                        if (_config.Data.WoundConfig.RagdollOnPainfulWound)
                        {
                            SetPedToRagdollEvent ragdoll;
                            _ecsWorld.CreateEntityWith(out ragdoll);
                            ragdoll.PedEntity = pedEntity;
                            ragdoll.RagdollState = RagdollStates.SHORT;
                        }
                        
                        if (woundedPed.IsPlayer)
                        {
                            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
                            _ecsWorld.CreateEntityWith<AddPlayerAdrenalineEffectEvent>();
                        }
                    }
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }
    }
}