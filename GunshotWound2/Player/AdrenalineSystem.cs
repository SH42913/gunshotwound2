using GTA;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using GunshotWound2.Pain;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public sealed class AdrenalineSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(AdrenalineSystem);
#endif

            var playerEntity = _config.Data.PlayerConfig.PlayerEntity;
            if (!_ecsWorld.IsEntityExists(playerEntity)) return;

            var playerPed = _ecsWorld.GetComponent<WoundedPedComponent>(playerEntity);
            var pain = _ecsWorld.GetComponent<PainComponent>(playerEntity);
            if (playerPed == null || pain == null || playerPed.InPermanentRagdoll)
            {
                Game.TimeScale = 1f;
                return;
            }

            var painPercent = pain.CurrentPain / playerPed.MaximalPain;
            var adjustable = 1f - _config.Data.PlayerConfig.MaximalSlowMo;
            Game.TimeScale = painPercent <= 1f
                ? 1f - adjustable * painPercent
                : 1f;
        }
    }
}