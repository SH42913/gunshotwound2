using GunshotWound2.Components.EffectComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Components.WoundComponents.PainStateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.WoundSystems.PainStatesSystem
{
    [EcsInject]
    public class MildPainStateSystem : BasePainStateSystem<MildPainStateComponent>
    {
        public MildPainStateSystem()
        {
            CurrentState = PainStates.MILD;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SwitchAnimationComponent anim;
            EcsWorld.CreateEntityWith(out anim);
            anim.PedEntity = pedEntity;
            anim.AnimationName = woundedPed.IsPlayer 
                ? Config.Data.PlayerConfig.MildPainAnim
                : Config.Data.NpcConfig.MildPainAnim;
        }
    }
}