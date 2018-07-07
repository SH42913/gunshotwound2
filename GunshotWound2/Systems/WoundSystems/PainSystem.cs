using GTA.Native;
using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems
{
    [EcsInject]
    public class PainSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<PainComponent> _components;
        private EcsFilterSingle<WoundConfig> _config;
        
        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(PainSystem);
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    var additionalPain = _components.Components1[i].PainAmount;
                    woundedPed.PainMeter += additionalPain;

                    var painPercent = woundedPed.PainMeter / woundedPed.MaximalPain;
                    var backPercent = 1 - painPercent;
                    if (woundedPed.IsPlayer)
                    {
                        if (additionalPain > 40)
                        {
                            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
                        }

                        if (!woundedPed.DamagedParts.HasFlag(DamageTypes.ARMS_DAMAGED) &&
                            painPercent > 0.5f)
                        {
                            Function.Call(Hash._SET_CAM_EFFECT, 2);
                        }
                    }
                    else
                    {
                        if (!woundedPed.DamagedParts.HasFlag(DamageTypes.ARMS_DAMAGED))
                        {
                            woundedPed.ThisPed.Accuracy = (int) (backPercent * woundedPed.DefaultAccuracy);
                        }
                    }

                    if (!woundedPed.DamagedParts.HasFlag(DamageTypes.LEGS_DAMAGED))
                    {
                        var moveRate = _config.Data.MoveRateOnFullPain +
                                       (1 - _config.Data.MoveRateOnFullPain) * backPercent;
                        Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, moveRate);
                    }
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }
    }
}