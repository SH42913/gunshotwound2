using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class GutsCriticalSystem : BaseCriticalSystem<GutsCritcalWoundEvent>
    {
        public GutsCriticalSystem()
        {
            CurrentCrit = CritTypes.GUTS_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            
            SendMessage(Locale.Data.PlayerGutsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            Function.Call(Hash.CREATE_NM_MESSAGE, true, 1119);
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, pedComponent.ThisPed);
            
            SendMessage(pedComponent.IsMale 
                ? Locale.Data.ManGutsCritMessage 
                : Locale.Data.WomanGutsCritMessage);
        }
    }
}