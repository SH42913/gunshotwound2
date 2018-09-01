using GTA;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class AveragePainStateSystem : BasePainStateSystem<AverageChangePainStateEvent>
    {
        public AveragePainStateSystem()
        {
            CurrentState = PainStates.AVERAGE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SendPedToRagdoll(pedEntity, RagdollStates.WAKE_UP);
            ChangeWalkingAnimation(pedEntity, woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.AvgPainAnim
                : Config.Data.NpcConfig.AvgPainAnim);
            
            if (!woundedPed.IsPlayer) return;
            Game.Player.IgnoredByEveryone = false;
        }
    }
}