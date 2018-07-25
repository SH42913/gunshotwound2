using GTA;
using GunshotWound2.Components.EffectComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class UnbearablePainStateSystem : BasePainStateSystem<UnbearablePainStateComponent>
    {
        public UnbearablePainStateSystem()
        {
            CurrentState = PainStates.UNBEARABLE;
        }
        
        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            RagdollRequestComponent request;
            EcsWorld.CreateEntityWith(out request);
            request.PedEntity = pedEntity;
            request.Enable = true;
            
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