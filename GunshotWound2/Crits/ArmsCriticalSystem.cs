using GTA.Native;
using GunshotWound2.GUI;
using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using Leopotam.Ecs;

namespace GunshotWound2.Crits
{
    public sealed class ArmsCriticalWoundEvent : BaseCriticalWoundEvent
    {
    }

    [EcsInject]
    public sealed class ArmsCriticalSystem : BaseCriticalSystem<ArmsCriticalWoundEvent>
    {
        protected override CritTypes CurrentCrit => CritTypes.ARMS_DAMAGED;

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);

            EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.PERMANENT;
            if (Config.Data.PlayerConfig.CanDropWeapon)
            {
                pedComponent.ThisPed.Weapons.Drop();
            }

            if (pedComponent.Crits.Has(CritTypes.NERVES_DAMAGED)) return;
            SendMessage(Locale.Data.PlayerArmsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);

            pedComponent.ThisPed.Accuracy = (int) (0.1f * pedComponent.DefaultAccuracy);

            pedComponent.ThisPed.Weapons.Drop();
            Function.Call(Hash.CREATE_NM_MESSAGE, true, 0);
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, pedComponent.ThisPed);
            Function.Call(Hash.CREATE_NM_MESSAGE, true, 1024);
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, pedComponent.ThisPed);

            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            if (pedComponent.Crits.Has(CritTypes.NERVES_DAMAGED)) return;
            SendMessage(pedComponent.IsMale
                ? Locale.Data.ManArmsCritMessage
                : Locale.Data.WomanArmsCritMessage);
        }
    }
}