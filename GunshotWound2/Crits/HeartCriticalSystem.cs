using GTA.Native;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
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

            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManHeartCritMessage
                : Locale.Data.WomanHeartCritMessage);
        }
    }
}