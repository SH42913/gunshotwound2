using GTA;
using GTA.Native;
using GunshotWound2.Effects;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public sealed class LegsCriticalWoundEvent : BaseCriticalWoundEvent
    {
    }

    [EcsInject]
    public sealed class LegsCriticalSystem : BaseCriticalSystem<LegsCriticalWoundEvent>
    {
        protected override CritTypes CurrentCrit => CritTypes.LEGS_DAMAGED;

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 20);

            Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, false);
            Function.Call(
                Hash.SET_PED_MOVE_RATE_OVERRIDE,
                pedComponent.ThisPed,
                Config.Data.WoundConfig.MoveRateOnNervesDamage);

            SendPedToRagdoll(pedComponent, pedEntity);

            if (pedComponent.Crits.Has(CritTypes.NERVES_DAMAGED)) return;
            SendMessage(Locale.Data.PlayerLegsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 20);

            Function.Call(
                Hash.SET_PED_MOVE_RATE_OVERRIDE,
                pedComponent.ThisPed,
                Config.Data.WoundConfig.MoveRateOnNervesDamage);

            SendPedToRagdoll(pedComponent, pedEntity);

            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            if (pedComponent.Crits.Has(CritTypes.NERVES_DAMAGED)) return;
            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManLegsCritMessage
                : Locale.Data.WomanLegsCritMessage);
        }

        private void SendPedToRagdoll(WoundedPedComponent pedComponent, int pedEntity)
        {
            if (!pedComponent.ThisPed.IsRunning && !pedComponent.ThisPed.IsSprinting) return;

            SendPedToRagdoll(pedEntity, RagdollStates.LEG_DAMAGE);
        }
    }
}