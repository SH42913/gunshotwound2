using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class DeadlyPainStateSystem : BasePainStateSystem<DeadlyPainChangeStateEvent>
    {
        public DeadlyPainStateSystem()
        {
            CurrentState = PainStates.DEADLY;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            if (woundedPed.IsPlayer)
            {
                woundedPed.PedHealth = Config.Data.PlayerConfig.MinimalHealth - 1;
                SendMessage(Locale.Data.PainShockDeath, NotifyLevels.EMERGENCY);
            }
            else
            {
                woundedPed.PedHealth = -1;
            }
        }
    }
}