using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class StomachCriticalSystem : BaseCriticalSystem<StomachCriticalWoundEvent>
    {
        public StomachCriticalSystem()
        {
            CurrentCrit = CritTypes.STOMACH_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            
            SendMessage(Locale.Data.PlayerStomachCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            
            SendMessage(pedComponent.IsMale 
                ? Locale.Data.ManStomachCritMessage 
                : Locale.Data.WomanStomachCritMessage);
        }
    }
}