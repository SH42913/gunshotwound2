using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public sealed class StomachCriticalWoundEvent : BaseCriticalWoundEvent
    {
    }

    [EcsInject]
    public sealed class StomachCriticalSystem : BaseCriticalSystem<StomachCriticalWoundEvent>
    {
        protected override CritTypes CurrentCrit => CritTypes.STOMACH_DAMAGED;

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