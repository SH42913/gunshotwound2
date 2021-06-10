using GunshotWound2.Effects;
using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class NoPainStateSystem : BasePainStateSystem<NoPainChangeStateEvent>
    {
        public NoPainStateSystem()
        {
            CurrentState = PainStates.NONE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SendPedToRagdoll(pedEntity, RagdollStates.WAKE_UP);
            ResetMoveSet(pedEntity);

            if (!woundedPed.IsPlayer) return;
            if (!woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED))
            {
                EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.CLEAR;
            }

            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = false;
        }
    }
}