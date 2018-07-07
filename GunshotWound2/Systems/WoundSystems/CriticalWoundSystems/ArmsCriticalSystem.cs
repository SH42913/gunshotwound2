using GTA.Native;
using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class ArmsCriticalSystem : BaseCriticalSystem<ArmsCriticalComponent>
    {
        public ArmsCriticalSystem()
        {
            Damage = DamageTypes.ARMS_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            Function.Call(Hash._SET_CAM_EFFECT, 2);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            pedComponent.ThisPed.Accuracy = (int) (0.2f * pedComponent.DefaultAccuracy);
            if(!NpcConfig.Data.ShowEnemyCriticalMessages ||
               pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)) return;
            SendMessage($"{(pedComponent.IsMale ? "His" : "Her")} arm looks very bad");
        }
    }
}