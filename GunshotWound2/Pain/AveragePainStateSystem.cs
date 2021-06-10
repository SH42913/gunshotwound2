using GTA;
using GunshotWound2.Effects;
using GunshotWound2.HitDetection;
using GunshotWound2.Player;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class AveragePainStateSystem : BasePainStateSystem<AveragePainChangeStateEvent>
    {
        public AveragePainStateSystem()
        {
            CurrentState = PainStates.AVERAGE;
        }

        protected override void ExecuteState(WoundedPedComponent woundedPed, int pedEntity)
        {
            base.ExecuteState(woundedPed, pedEntity);

            SendPedToRagdoll(pedEntity, RagdollStates.WAKE_UP);
            ChangeMoveSet(pedEntity, woundedPed.IsPlayer
                ? Config.Data.PlayerConfig.AvgPainSets
                : Config.Data.NpcConfig.AvgPainSets);

            if (!woundedPed.IsPlayer) return;
            Game.Player.IgnoredByEveryone = false;
            EcsWorld.CreateEntityWith<ChangeSpecialAbilityEvent>().Lock = true;
        }
    }
}