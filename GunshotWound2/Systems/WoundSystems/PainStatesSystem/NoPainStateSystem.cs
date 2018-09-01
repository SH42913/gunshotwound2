using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class NoPainStateSystem : BasePainStateSystem<NoPainChangeStateEvent>
    {
        public NoPainStateSystem()
        {
            CurrentState = PainStates.NONE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SendPedToRagdoll(pedEntity, RagdollStates.WAKE_UP);
            ChangeWalkingAnimation(pedEntity, woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.NoPainAnim
                : Config.Data.NpcConfig.NoPainAnim);
            
            if(!woundedPed.IsPlayer) return;
            Function.Call(Hash._SET_CAM_EFFECT, 0);
        }
    }
}