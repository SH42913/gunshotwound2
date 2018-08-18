using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class LungsCriticalSystem : BaseCriticalSystem<HeartCriticalWoundEvent>
    {
        public LungsCriticalSystem()
        {
            Damage = DamageTypes.LUNGS_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 10f);
            CreateInternalBleeding(pedEntity, 0.5f);
            
            Function.Call(Hash._START_SCREEN_EFFECT, "DrugsDrivingIn", 5000, true);
            Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, false);
            SendMessage("It\'s very hard for you to breathe", NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 10f);
            CreateInternalBleeding(pedEntity, 0.5f);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage($"{(pedComponent.HeShe)} coughs up blood");
        }
    }
}