using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems.CriticalWoundSystems
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