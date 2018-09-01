using GTA.Native;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class MildPainStateSystem : BasePainStateSystem<MildChangePainStateEvent>
    {
        public MildPainStateSystem()
        {
            CurrentState = PainStates.MILD;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);
            
            ChangeWalkingAnimation(pedEntity, woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.MildPainAnim
                : Config.Data.NpcConfig.MildPainAnim);
            
            if(!woundedPed.IsPlayer) return;
            Function.Call(Hash._SET_CAM_EFFECT, 0);
        }
    }
}