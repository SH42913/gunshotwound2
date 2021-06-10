using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class IntensePainStateSystem : BasePainStateSystem<IntensePainChangeStateEvent>
    {
        public IntensePainStateSystem()
        {
            CurrentState = PainStates.INTENSE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            ChangeMoveSet(pedEntity, woundedPed.IsPlayer
                ? Config.Data.PlayerConfig.IntensePainSets
                : Config.Data.NpcConfig.IntensePainSets);

            if (woundedPed.Crits.Has(CritTypes.ARMS_DAMAGED)) return;
            var pain = EcsWorld.GetComponent<PainComponent>(pedEntity);
            var backPercent = 1f - pain.CurrentPain / woundedPed.MaximalPain;
            if (woundedPed.IsPlayer)
            {
                EcsWorld.CreateEntityWith<AddCameraShakeEvent>().Length = CameraShakeLength.PERMANENT;
                EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
            }
            else
            {
                woundedPed.ThisPed.Accuracy = (int) (backPercent * woundedPed.DefaultAccuracy);
            }
        }
    }
}