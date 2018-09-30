using GunshotWound2.Components.Events.GuiEvents;
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

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            if (woundedPed.IsPlayer)
            {
                woundedPed.ThisPed.Health = Config.Data.PlayerConfig.MinimalHealth - 1;
                SendMessage(Locale.Data.PainShockDeath, NotifyLevels.EMERGENCY);
            }
            else
            {
                woundedPed.ThisPed.Health = -1;
            }
        }
    }
}