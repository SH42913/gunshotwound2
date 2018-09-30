using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystems
{
    [EcsInject]
    public class MildPainStateSystem : BasePainStateSystem<MildPainChangeStateEvent>
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
            if (!woundedPed.Crits.HasFlag(CritTypes.ARMS_DAMAGED))
            {
                EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.CLEAR;
            }
            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = false;
        }
    }
}