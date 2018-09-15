using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class ArmsCriticalSystem : BaseCriticalSystem<ArmsCriticalWoundEvent>
    {
        public ArmsCriticalSystem()
        {
            CurrentCrit = CritTypes.ARMS_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);

            EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.PERMANENT;
            if (Config.Data.PlayerConfig.CanDropWeapon)
            {
                pedComponent.ThisPed.Weapons.Drop();
            }

            if (pedComponent.Crits.HasFlag(CritTypes.NERVES_DAMAGED)) return;
            SendMessage(Locale.Data.PlayerArmsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            
            pedComponent.ThisPed.Accuracy = (int) (0.1f * pedComponent.DefaultAccuracy);

            pedComponent.ThisPed.Weapons.Drop();
            
            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            if (pedComponent.Crits.HasFlag(CritTypes.NERVES_DAMAGED)) return;
            SendMessage(pedComponent.IsMale 
                            ? Locale.Data.ManArmsCritMessage
                            : Locale.Data.WomanArmsCritMessage);
        }
    }
}