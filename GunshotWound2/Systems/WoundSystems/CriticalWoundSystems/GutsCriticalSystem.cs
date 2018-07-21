using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class GutsCriticalSystem : BaseCriticalSystem<GutsCritcalComponent>
    {
        public GutsCriticalSystem()
        {
            Damage = DamageTypes.GUTS_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            SendMessage("You feel yourself very sick", NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            SendMessage($"{pedComponent.HeShe} looks very sick");
        }
    }
}