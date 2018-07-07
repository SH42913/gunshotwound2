using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class StomachCriticalSystem : BaseCriticalSystem<StomachCriticalComponent>
    {
        public StomachCriticalSystem()
        {
            Damage = DamageTypes.STOMACH_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
        }
    }
}