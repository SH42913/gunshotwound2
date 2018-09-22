using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems
{
    [EcsInject]
    public class PainRecoverySystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<WoundedPedComponent, PainComponent> _pedsWithPain;
        private EcsFilterSingle<MainConfig> _config;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(PainRecoverySystem);
#endif
            float frameTimeInSeconds = Game.LastFrameTime;
            
            for (int i = 0; i < _pedsWithPain.EntitiesCount; i++)
            {
                WoundedPedComponent woundedPed = _pedsWithPain.Components1[i];
                PainComponent pain = _pedsWithPain.Components2[i];
                
                int pedEntity = _pedsWithPain.Entities[i];
                if (pain.CurrentPain <= 0f)
                {
                    _ecsWorld.RemoveComponent<PainComponent>(pedEntity);
                    continue;
                }
                
                pain.CurrentPain -= woundedPed.PainRecoverSpeed * frameTimeInSeconds;
                var painPercent = pain.CurrentPain / woundedPed.MaximalPain;
                var backPercent = painPercent > 1
                    ? 0
                    : 1 - painPercent;

                /*if (painPercent > 3f)
                {
                    if(woundedPed.PainState == PainStates.DEADLY) continue;

                    _ecsWorld.CreateEntityWith(out UnbearablePainChangeStateEvent unbearablePainEvent);
                    unbearablePainEvent.PedEntity = pedEntity;
                    unbearablePainEvent.ForceUpdate = false;
                }
                else */
                if (painPercent > 1f)
                {
                    if(woundedPed.PainState == PainStates.UNBEARABLE) continue;

                    _ecsWorld.CreateEntityWith(out UnbearablePainChangeStateEvent unbearablePainEvent);
                    unbearablePainEvent.Entity = pedEntity;
                    unbearablePainEvent.ForceUpdate = false;
                }
                else if(painPercent > 0.7f)
                {
                    if(woundedPed.PainState == PainStates.INTENSE) continue;

                    _ecsWorld.CreateEntityWith(out IntensePainChangeStateEvent intensePainEvent);
                    intensePainEvent.Entity = pedEntity;
                    intensePainEvent.ForceUpdate = false;
                }
                else if (painPercent > 0.3f)
                {
                    if(woundedPed.PainState == PainStates.AVERAGE) continue;

                    _ecsWorld.CreateEntityWith(out AveragePainChangeStateEvent averagePainEvent);
                    averagePainEvent.Entity = pedEntity;
                    averagePainEvent.ForceUpdate = false;
                }
                else if (painPercent > 0.1f)
                {
                    if (woundedPed.PainState == PainStates.MILD) continue;

                    _ecsWorld.CreateEntityWith(out MildPainChangeStateEvent mildPainEvent);
                    mildPainEvent.Entity = pedEntity;
                    mildPainEvent.ForceUpdate = false;
                }
                else
                {
                    if(woundedPed.PainState == PainStates.NONE) continue;

                    _ecsWorld.CreateEntityWith(out NoPainChangeStateEvent noPainEvent);
                    noPainEvent.Entity = pedEntity;
                    noPainEvent.ForceUpdate = false;
                }

                if (woundedPed.Crits.HasFlag(CritTypes.LEGS_DAMAGED)) continue;

                var adjustable = 1f - _config.Data.WoundConfig.MoveRateOnFullPain;
                var moveRate = 1f - adjustable * backPercent;
                Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, moveRate);
            }
        }
    }
}