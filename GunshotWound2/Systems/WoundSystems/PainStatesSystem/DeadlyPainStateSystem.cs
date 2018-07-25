using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class DeadlyPainStateSystem : BasePainStateSystem<DeadlyPainStateComponent>
    {
        public DeadlyPainStateSystem()
        {
            CurrentState = PainStates.DEADLY;
        }
    }
}