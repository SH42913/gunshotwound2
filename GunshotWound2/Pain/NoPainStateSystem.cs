using GTA.Native;
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
            ChangeWalkingAnimation(pedEntity, woundedPed.IsPlayer
                ? Config.Data.PlayerConfig.NoPainAnim
                : Config.Data.NpcConfig.NoPainAnim);
            if (woundedPed.IsMale)
            {
                Function.Call(Hash.PLAY_FACIAL_ANIM, woundedPed.ThisPed, "mood_happy_1", "facials@gen_male@base");
            }
            else
            {
                Function.Call(Hash.PLAY_FACIAL_ANIM, woundedPed.ThisPed, "mood_happy_1", "facials@gen_female@base");
            }

            if (!woundedPed.IsPlayer) return;
            if (!woundedPed.Crits.HasFlag(CritTypes.ARMS_DAMAGED))
            {
                EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.CLEAR;
            }

            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = false;
        }
    }
}