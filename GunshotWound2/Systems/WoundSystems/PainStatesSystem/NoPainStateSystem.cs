using GunshotWound2.Components.EffectComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class NoPainStateSystem : BasePainStateSystem<NoPainStateComponent>
    {
        public NoPainStateSystem()
        {
            CurrentState = PainStates.NONE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SwitchAnimationComponent anim;
            EcsWorld.CreateEntityWith(out anim);
            anim.PedEntity = pedEntity;
            anim.AnimationName = woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.NoPainAnim
                : Config.Data.NpcConfig.NoPainAnim;
            
            RagdollRequestComponent request;
            EcsWorld.CreateEntityWith(out request);
            request.PedEntity = pedEntity;
            request.Enable = false;
        }
    }
}