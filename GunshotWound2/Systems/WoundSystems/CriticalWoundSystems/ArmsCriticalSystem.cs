using GTA.Native;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
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
            CreatePain(pedEntity, 25f);
            Function.Call(Hash._SET_CAM_EFFECT, 2);
            pedComponent.ThisPed.Weapons.Drop();

            if (!pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED))
            {
                SendMessage("You feel awful pain in your arm", NotifyLevels.WARNING);
            }
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            pedComponent.ThisPed.Accuracy = (int) (0.1f * pedComponent.DefaultAccuracy);
            pedComponent.ThisPed.Weapons.Drop();
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage($"{pedComponent.HisHer} arm looks very bad");
        }
    }
}