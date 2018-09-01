using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
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
            Damage = DamageTypes.ARMS_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            
            Function.Call(Hash._SET_CAM_EFFECT, 2);
            pedComponent.ThisPed.Weapons.Drop();

            if (pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)) return;
            SendMessage(Locale.Data.PlayerArmsCritMessage, NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            
            pedComponent.ThisPed.Accuracy = (int) (0.1f * pedComponent.DefaultAccuracy);
            pedComponent.ThisPed.Weapons.Drop();
            
            if (!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            if (pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)) return;
            SendMessage(pedComponent.IsMale 
                            ? Locale.Data.ManArmsCritMessage
                            : Locale.Data.WomanArmsCritMessage);
        }
    }
}