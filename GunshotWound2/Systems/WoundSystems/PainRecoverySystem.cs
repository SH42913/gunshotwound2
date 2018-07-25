using GTA;
using GTA.Native;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class PainRecoverySystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<WoundedPedComponent> _peds;
        private EcsFilterSingle<MainConfig> _config;
        private uint _ticks;
        private float _lastTime;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(PainRecoverySystem);
            
            var ticksToRefresh = _config.Data.TicksToRefresh;
            _lastTime += Game.LastFrameTime;
            if(++_ticks % ticksToRefresh != 0) return;
            
            for (int i = 0; i < _peds.EntitiesCount; i++)
            {
                var woundedPed = _peds.Components1[i];
                int pedEntity = _peds.Entities[i];
                if(woundedPed.PainMeter <= 0.05f) continue;
                
                woundedPed.PainMeter -= woundedPed.PainRecoverSpeed * _lastTime;
                var painPercent = woundedPed.PainMeter / woundedPed.MaximalPain;
                var backPercent = painPercent > 1
                    ? 0
                    : 1 - painPercent;
                if (woundedPed.PainMeter < 0) woundedPed.PainMeter = 0;

                /*if (painPercent > 3f)
                {
                    if(woundedPed.PainState == PainStates.DEADLY) continue;

                    _ecsWorld.AddComponent<DeadlyPainStateComponent>(pedEntity);
                }
                else */
                if (painPercent > 1f)
                {
                    if(woundedPed.PainState == PainStates.UNBEARABLE) continue;

                    _ecsWorld
                        .CreateEntityWith<UnbearablePainStateComponent>()
                        .PedEntity = pedEntity;
                }
                else if(painPercent > 0.7f)
                {
                    if(woundedPed.PainState == PainStates.INTENSE) continue;

                    _ecsWorld
                        .CreateEntityWith<IntensePainStateComponent>()
                        .PedEntity = pedEntity;
                }
                else if (painPercent > 0.3f)
                {
                    if(woundedPed.PainState == PainStates.AVERAGE) continue;

                    _ecsWorld
                        .CreateEntityWith<AveragePainStateComponent>()
                        .PedEntity = pedEntity;
                }
                else if (painPercent > 0.1f)
                {
                    if (woundedPed.PainState == PainStates.MILD) continue;

                    _ecsWorld
                        .CreateEntityWith<MildPainStateComponent>()
                        .PedEntity = pedEntity;
                }
                else
                {
                    if(woundedPed.PainState == PainStates.NONE) continue;

                    _ecsWorld
                        .CreateEntityWith<NoPainStateComponent>()
                        .PedEntity = pedEntity;
                }

                if (woundedPed.DamagedParts.HasFlag(DamageTypes.LEGS_DAMAGED)) continue;
                
                var moveRate = _config.Data.WoundConfig.MoveRateOnFullPain +
                               (1 - _config.Data.WoundConfig.MoveRateOnFullPain) * backPercent;
                Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, moveRate);
            }

            _lastTime = 0;
        }
    }
}