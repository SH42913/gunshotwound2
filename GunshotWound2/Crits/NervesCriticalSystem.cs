using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    [EcsInject]
    public sealed class NervesCriticalSystem : BaseCriticalSystem<NervesCriticalWoundEvent>
    {
        public NervesCriticalSystem()
        {
            CurrentCrit = CritTypes.NERVES_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendMessage(Locale.Data.PlayerNervesCritMessage, NotifyLevels.WARNING);
            SendToRagdollOrArmLegsDamage(pedEntity);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            SendToRagdollOrArmLegsDamage(pedEntity);

            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
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
            EcsWorld.CreateEntityWith<ArmsCriticalWoundEvent>().Entity = pedEntity;
            EcsWorld.CreateEntityWith<LegsCriticalWoundEvent>().Entity = pedEntity;
        }
    }
}