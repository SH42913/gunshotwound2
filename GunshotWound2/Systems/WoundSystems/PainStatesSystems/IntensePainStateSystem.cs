﻿using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystems
{
    [EcsInject]
    public class IntensePainStateSystem : BasePainStateSystem<IntensePainChangeStateEvent>
    {
        public IntensePainStateSystem()
        {
            CurrentState = PainStates.INTENSE;
        }
        
        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            ChangeWalkingAnimation(pedEntity, woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.IntensePainAnim
                : Config.Data.NpcConfig.IntensePainAnim);
            
            if (woundedPed.Crits.HasFlag(CritTypes.ARMS_DAMAGED)) return;
            var backPercent = 1f - woundedPed.PainMeter / woundedPed.MaximalPain;
            if (woundedPed.IsPlayer)
            {
                EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.PERMANENT;
                EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
            }
            else
            {
                woundedPed.ThisPed.Accuracy = (int) (backPercent * woundedPed.DefaultAccuracy);
            }
        }
    }
}