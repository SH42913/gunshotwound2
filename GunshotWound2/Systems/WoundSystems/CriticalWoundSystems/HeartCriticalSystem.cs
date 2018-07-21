using GTA.Native;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.CriticalWoundComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class HeartCriticalSystem : BaseCriticalSystem<HeartCriticalComponent>
    {
        public HeartCriticalSystem()
        {
            Damage = DamageTypes.HEART_DAMAGED;
        }

        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2f);
            Function.Call(Hash._START_SCREEN_EFFECT, "DrugsDrivingIn", 5000, true);
            SendMessage("You feel like life leaving you", NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            CreatePain(pedEntity, 25f);
            CreateInternalBleeding(pedEntity, 2f);
            
            pedComponent.ThisPed.Task.ClearAllImmediately();
            pedComponent.ThisPed.Task.StandStill(5);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages) return;
            SendMessage($"{(pedComponent.HeShe)} coughs up blood");
        }
    }
}