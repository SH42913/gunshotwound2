using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystems
{
    [EcsInject]
    public class DeadlyPainStateSystem : BasePainStateSystem<DeadlyPainChangeStateEvent>
    {
        public DeadlyPainStateSystem()
        {
            CurrentState = PainStates.DEADLY;
        }
    }
}