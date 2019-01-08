using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class HeartCriticalSystem : BaseCriticalSystem<HeartCriticalWoundEvent>
    {
        public HeartCriticalSystem()
        {
            CurrentCrit = CritTypes.HEART_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2.5f);
            
            SendPedToRagdoll(pedEntity, RagdollStates.HEART_DAMAGE);
            Function.Call(Hash._START_SCREEN_EFFECT, "DrugsDrivingIn", 5000, true);
            
            SendMessage(Locale.Data.PlayerHeartCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2f);
            
            SendPedToRagdoll(pedEntity, RagdollStates.HEART_DAMAGE);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage(pedComponent.IsMale 
                ? Locale.Data.ManHeartCritMessage 
                : Locale.Data.WomanHeartCritMessage);
        }
    }
}