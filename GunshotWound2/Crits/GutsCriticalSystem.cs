using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public sealed class GutsCriticalWoundEvent : BaseCriticalWoundEvent
    {
    }

    [EcsInject]
    public sealed class GutsCriticalSystem : BaseCriticalSystem<GutsCriticalWoundEvent>
    {
        protected override CritTypes CurrentCrit => CritTypes.GUTS_DAMAGED;

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);

            SendPedToRagdoll(pedEntity, RagdollStates.GUTS_DAMAGE);
            StartPostFx("DrugsDrivingIn", 5000);

            SendMessage(Locale.Data.PlayerGutsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            SendPedToRagdoll(pedEntity, RagdollStates.GUTS_DAMAGE);

            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManGutsCritMessage
                : Locale.Data.WomanGutsCritMessage);
        }
    }
}