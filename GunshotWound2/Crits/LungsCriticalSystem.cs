using GTA;
using GTA.Native;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    [EcsInject]
    public class LungsCriticalSystem : BaseCriticalSystem<HeartCriticalWoundEvent>
    {
        public LungsCriticalSystem()
        {
            CurrentCrit = CritTypes.LUNGS_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 10f);
            CreateInternalBleeding(pedEntity, 2f);

            Function.Call(Hash._START_SCREEN_EFFECT, "DrugsDrivingIn", 5000, true);
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