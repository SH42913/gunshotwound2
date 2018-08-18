using GunshotWound2.Components.EffectComponents;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
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
            SendMessage("You feel you can\'t control arms and legs anymore", NotifyLevels.WARNING);
            SendToRagdollOrArmLegsDamage(pedEntity);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendToRagdollOrArmLegsDamage(pedEntity);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage($"{pedComponent.HeShe} looks {pedComponent.HisHer.ToLower()} spine damaged");
        }

        private void SendToRagdollOrArmLegsDamage(int pedEntity)
        {
            if (Config.Data.WoundConfig.RealisticNervesDamage)
            {
                RagdollRequestComponent request;
                EcsWorld.CreateEntityWith(out request);
                request.PedEntity = pedEntity;
                request.RagdollState = RagdollStates.PERMANENT;
            }
            else
            {
                SendArmsLegsDamage(pedEntity);
            }
        }

        private void SendArmsLegsDamage(int pedEntity)
        {
            EcsWorld.CreateEntityWith<ArmsCriticalComponent>().PedEntity = pedEntity;
            EcsWorld.CreateEntityWith<LegsCriticalComponent>().PedEntity = pedEntity;
        }
    }
}