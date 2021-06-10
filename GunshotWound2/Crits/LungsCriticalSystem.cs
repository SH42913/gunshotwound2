using GTA;
using GTA.Native;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public sealed class LungsCriticalWoundEvent : BaseCriticalWoundEvent
    {
    }

    [EcsInject]
    public sealed class LungsCriticalSystem : BaseCriticalSystem<HeartCriticalWoundEvent>
    {
        protected override CritTypes CurrentCrit => CritTypes.LUNGS_DAMAGED;

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 10f);
            CreateInternalBleeding(pedEntity, 2f);

            StartPostFx("DrugsDrivingIn", 5000);
            Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, false);

            SendMessage(Locale.Data.PlayerLungsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 10f);
            CreateInternalBleeding(pedEntity, 0.5f);

            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManLungsCritMessage
                : Locale.Data.WomanLungsCritMessage);
        }
    }
}