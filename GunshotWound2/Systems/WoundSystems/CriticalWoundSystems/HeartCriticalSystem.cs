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
            Damage = DamageTypes.HEART_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2f);
            SendPedToRagdoll(pedComponent, pedEntity);
            Function.Call(Hash._START_SCREEN_EFFECT, "DrugsDrivingIn", 5000, true);
            SendMessage("You feel awful pain in your chest", NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2f);
            SendPedToRagdoll(pedComponent, pedEntity);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage($"{(pedComponent.HeShe)} coughs up blood");
        }

        private void SendPedToRagdoll(WoundedPedComponent pedComponent, int pedEntity)
        {
            SetPedToRagdollEvent ragdoll;
            EcsWorld.CreateEntityWith(out ragdoll);
            ragdoll.PedEntity = pedEntity;
            ragdoll.RagdollState = RagdollStates.HEART_DAMAGE;
        }
    }
}