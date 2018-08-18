using GTA;
using GunshotWound2.Components.EffectComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class AveragePainStateSystem : BasePainStateSystem<AveragePainStateComponent>
    {
        public AveragePainStateSystem()
        {
            CurrentState = PainStates.AVERAGE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            RagdollRequestComponent request;
            EcsWorld.CreateEntityWith(out request);
            request.PedEntity = pedEntity;
            request.RagdollState = RagdollStates.WAKE_UP;

            SwitchAnimationComponent anim;
            EcsWorld.CreateEntityWith(out anim);
            anim.PedEntity = pedEntity;
            anim.AnimationName = woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.AvgPainAnim
                : Config.Data.NpcConfig.AvgPainAnim;
            
            if (!woundedPed.IsPlayer) return;
            Game.Player.IgnoredByEveryone = false;
        }
    }
}