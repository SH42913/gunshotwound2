using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class NervesCriticalSystem : BaseCriticalSystem<NervesCriticalWoundEvent>
    {
        public NervesCriticalSystem()
        {
            Damage = DamageTypes.NERVES_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendMessage(Locale.Data.PlayerNervesCritMessage, NotifyLevels.WARNING);
            SendToRagdollOrArmLegsDamage(pedEntity);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendToRagdollOrArmLegsDamage(pedEntity);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage(pedComponent.IsMale 
                ? Locale.Data.ManNervesCritMessage 
                : Locale.Data.WomanNervesCritMessage);
        }

        private void SendToRagdollOrArmLegsDamage(int pedEntity)
        {
            if (Config.Data.WoundConfig.RealisticNervesDamage)
            {
                SendPedToRagdoll(pedEntity, RagdollStates.PERMANENT);
            }
            else
            {
                SendArmsLegsCrit(pedEntity);
            }
        }

        private void SendArmsLegsCrit(int pedEntity)
        {
            EcsWorld.CreateEntityWith<ArmsCriticalWoundEvent>().PedEntity = pedEntity;
            EcsWorld.CreateEntityWith<LegsCriticalWoundEvent>().PedEntity = pedEntity;
        }
    }
}