using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class NervesCriticalSystem : BaseCriticalSystem<NervesCriticalComponent>
    {
        public NervesCriticalSystem()
        {
            Damage = DamageTypes.NERVES_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendArmsLegsDamage(pedEntity);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendArmsLegsDamage(pedEntity);
        }

        private void SendArmsLegsDamage(int pedEntity)
        {
            EcsWorld.CreateEntityWith<ArmsCriticalComponent>().PedEntity = pedEntity;
            EcsWorld.CreateEntityWith<LegsCriticalComponent>().PedEntity = pedEntity;
        }
    }
}