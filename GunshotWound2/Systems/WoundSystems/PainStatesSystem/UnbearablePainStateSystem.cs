﻿using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class UnbearablePainStateSystem : BasePainStateSystem<UnbearableChangePainStateEvent>
    {
        public UnbearablePainStateSystem()
        {
            CurrentState = PainStates.UNBEARABLE;
        }
        
        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SetPedToRagdollEvent request;
            EcsWorld.CreateEntityWith(out request);
            request.PedEntity = pedEntity;
            request.RagdollState = RagdollStates.PERMANENT;
            
            woundedPed.ThisPed.Weapons.Drop();
            
            if (!woundedPed.IsPlayer) return;

            Game.Player.IgnoredByEveryone = true;
            if (Config.Data.PlayerConfig.PoliceCanForgetYou) Game.Player.WantedLevel = 0;
            if (!woundedPed.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED) && !woundedPed.IsDead)
            {
                SendMessage("You can't take this pain anymore!\n" +
                            "You lose consciousness!", NotifyLevels.WARNING);
            }
        }
    }
}