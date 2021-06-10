using GTA.Native;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public sealed class HeartCriticalWoundEvent : BaseCriticalWoundEvent
    {
    }

    [EcsInject]
    public sealed class HeartCriticalSystem : BaseCriticalSystem<HeartCriticalWoundEvent>
    {
        protected override CritTypes CurrentCrit => CritTypes.HEART_DAMAGED;

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2.5f);

            SendPedToRagdoll(pedEntity, RagdollStates.HEART_DAMAGE);
            StartPostFx("DrugsDrivingIn", 5000);

            SendMessage(Locale.Data.PlayerHeartCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2f);

            SendPedToRagdoll(pedEntity, RagdollStates.HEART_DAMAGE);

            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManHeartCritMessage
                : Locale.Data.WomanHeartCritMessage);
        }
    }
}