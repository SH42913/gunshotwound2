using GTA;
using GTA.Native;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PedEvents;
using GunshotWound2.Components.Events.WoundEvents.CriticalWoundEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.CriticalWoundSystems
{
    [EcsInject]
    public class LegsCriticalSystem : BaseCriticalSystem<LegsCriticalWoundEvent>
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
                Config.Data.WoundConfig.MoveRateOnNervesDamage);
            
            SendPedToRagdoll(pedComponent, pedEntity);

            if (pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)) return;
            
            SendMessage("You feel awful pain in your leg", NotifyLevels.WARNING);
        }

        protected override void ActionForNpc(WoundedPedComponent pedComponent, int pedEntity)
        {
            Function.Call(
                Hash.SET_PED_MOVE_RATE_OVERRIDE,
                pedComponent.ThisPed, 
                Config.Data.WoundConfig.MoveRateOnNervesDamage);
            
            SendPedToRagdoll(pedComponent, pedEntity);
            
            if(!Config.Data.NpcConfig.ShowEnemyCriticalMessages ||
               pedComponent.DamagedParts.HasFlag(DamageTypes.NERVES_DAMAGED)) return;
            
            SendMessage($"{pedComponent.HisHer} leg looks very bad");
        }

        private void SendPedToRagdoll(WoundedPedComponent pedComponent, int pedEntity)
        {
            if (!pedComponent.ThisPed.IsWalking && !pedComponent.ThisPed.IsRunning) return;
            
            SetPedToRagdollEvent ragdoll;
            EcsWorld.CreateEntityWith(out ragdoll);
            ragdoll.PedEntity = pedEntity;
            ragdoll.RagdollState = RagdollStates.LEG_DAMAGE;
        }
    }
}