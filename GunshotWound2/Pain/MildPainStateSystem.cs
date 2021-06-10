using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class MildPainStateSystem : BasePainStateSystem<MildPainChangeStateEvent>
    {
        public MildPainStateSystem()
        {
            CurrentState = PainStates.MILD;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            ChangeMoveSet(pedEntity, woundedPed.IsPlayer
                ? Config.Data.PlayerConfig.MildPainSets
                : Config.Data.NpcConfig.MildPainSets);

            if (!woundedPed.IsPlayer) return;
            if (!woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED))
            {
                EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.CLEAR;
            }

            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = false;
        }
    }
}