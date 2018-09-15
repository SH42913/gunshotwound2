using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents.ChangePainStateEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystems
{
    [EcsInject]
    public class UnbearablePainStateSystem : BasePainStateSystem<UnbearablePainChangeStateEvent>
    {
        public UnbearablePainStateSystem()
        {
            CurrentState = PainStates.UNBEARABLE;
        }
        
        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SendPedToRagdoll(pedEntity, RagdollStates.PERMANENT);
            if (woundedPed.IsPlayer && Config.Data.PlayerConfig.CanDropWeapon)
            {
                woundedPed.ThisPed.Weapons.Drop();
            }
            else if(!woundedPed.IsPlayer)
            {
                woundedPed.ThisPed.Weapons.Drop();
            }
            
            if (!woundedPed.IsPlayer) return;
            Game.Player.IgnoredByEveryone = true;
            Game.Player.CanControlCharacter = false;
            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
            
            if (Config.Data.PlayerConfig.PoliceCanForgetYou) Game.Player.WantedLevel = 0;
            
            if (woundedPed.Crits.HasFlag(CritTypes.NERVES_DAMAGED) || woundedPed.IsDead) return;
            SendMessage(Locale.Data.UnbearablePainMessage, NotifyLevels.WARNING);
        }
    }
}