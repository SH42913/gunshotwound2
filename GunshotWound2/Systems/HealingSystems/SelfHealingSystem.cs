using GTA;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HealingSystems
{
    [EcsInject]
    public class SelfHealingSystem : IEcsRunSystem
    {
        private EcsFilter<WoundedPedComponent> _woundedPeds;
        private EcsFilterSingle<MainConfig> _config;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(SelfHealingSystem);
#endif
            
            for (int i = 0; i < _woundedPeds.EntitiesCount; i++)
            {
                WoundedPedComponent woundedPed = _woundedPeds.Components1[i];
                
                var healthPercent = !woundedPed.IsPlayer
                    ? (float) woundedPed.ThisPed.Health / woundedPed.ThisPed.MaxHealth
                    : (woundedPed.Health - _config.Data.PlayerConfig.MinimalHealth) /
                      (_config.Data.PlayerConfig.MaximalHealth - _config.Data.PlayerConfig.MinimalHealth);
                if(woundedPed.BleedingCount > 0 || healthPercent > 0.95f) continue;

                woundedPed.Health += 0.01f * Game.LastFrameTime;
                woundedPed.ThisPed.Health = (int) woundedPed.Health;
            }
        }
    }
}