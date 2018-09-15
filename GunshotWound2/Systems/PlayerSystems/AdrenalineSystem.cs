using GTA;
using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.Events.PlayerEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.PlayerSystems
{
    [EcsInject]
    public class AdrenalineSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _config;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(AdrenalineSystem);
#endif

            var playerPed = _ecsWorld.GetComponent<WoundedPedComponent>(_config.Data.PlayerConfig.PlayerEntity);
            if (playerPed == null) return;

            var painPercent = playerPed.PainMeter / playerPed.MaximalPain;
            var adjustable = 1f - _config.Data.PlayerConfig.MaximalSlowMo;
            Game.TimeScale = painPercent <= 1f 
                ? 1f - adjustable * painPercent 
                : 1f;
        }
    }
}