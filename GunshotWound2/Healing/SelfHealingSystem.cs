using GTA;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Healing
{
    [EcsInject]
    public sealed class SelfHealingSystem : IEcsRunSystem
    {
        private readonly EcsFilter<WoundedPedComponent> _woundedPeds = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(SelfHealingSystem);
#endif

            for (var i = 0; i < _woundedPeds.EntitiesCount; i++)
            {
                var woundedPed = _woundedPeds.Components1[i];
                var needToHeal = woundedPed.IsPlayer
                    ? woundedPed.PedHealth + 1 < woundedPed.PedMaxHealth
                    : woundedPed.PedHealth < woundedPed.PedMaxHealth;
                if (woundedPed.IsDead || woundedPed.BleedingCount > 0 || !needToHeal) continue;

                woundedPed.Health += _config.Data.WoundConfig.SelfHealingRate * Game.LastFrameTime;
                woundedPed.PedHealth = woundedPed.Health;
            }
        }
    }
}