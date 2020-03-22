using GTA.Native;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    [EcsInject]
    public sealed class GutsCriticalSystem : BaseCriticalSystem<GutsCritcalWoundEvent>
    {
        public GutsCriticalSystem()
        {
            CurrentCrit = CritTypes.GUTS_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);

            SendMessage(Locale.Data.PlayerGutsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 0.5f);
            Function.Call(Hash.CREATE_NM_MESSAGE, true, 1119);
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, pedComponent.ThisPed);

            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManGutsCritMessage
                : Locale.Data.WomanGutsCritMessage);
        }
    }
}