using GTA;
using GTA.Native;
using GunshotWoundEcs.Components.WoundComponents;
using GunshotWoundEcs.Components.WoundComponents.CriticalWoundComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class LegsCriticalSystem : BaseCriticalSystem<LegsCriticalComponent>
    {
        public LegsCriticalSystem()
        {
            Damage = DamageTypes.LEGS_DAMAGED;
        }
        
        protected override void ActionForPlayer(WoundedPedComponent pedComponent, int pedEntity)
        {
            Function.Call(Hash.SET_PLAYER_SPRINT, Game.Player, false);
            Function.Call(
                Hash.SET_PED_MOVE_RATE_OVERRIDE,
                pedComponent.ThisPed, 
                WoundConfig.Data.MoveRateOnNervesDamage);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            Function.Call(
                Hash.SET_PED_MOVE_RATE_OVERRIDE,
                pedComponent.ThisPed, 
                WoundConfig.Data.MoveRateOnNervesDamage);
            
            if(!NpcConfig.Data.ShowEnemyCriticalMessages ||
               pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)) return;
            SendMessage($"{(pedComponent.IsMale ? "His" : "Her")} arm looks very bad");
        }
    }
}