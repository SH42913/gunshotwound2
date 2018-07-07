using GTA.Native;
using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems.CriticalWoundSystems
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
            Function.Call(Hash._START_SCREEN_EFFECT, "DrugsDrivingIn", 5000, true);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            if(!NpcConfig.Data.ShowEnemyCriticalMessages) return;
            SendMessage($"{(pedComponent.HeShe)} coughs up blood");
        }
    }
}