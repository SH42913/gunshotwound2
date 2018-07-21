using GTA;
using GTA.Native;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class LungsCriticalSystem : BaseCriticalSystem<HeartCriticalComponent>
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