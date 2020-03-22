using GTA;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Healing
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
                Ped ped = woundedPed.ThisPed;
                bool needToHeal = woundedPed.IsPlayer
                    ? ped.Health + 102 < ped.MaxHealth
                    : ped.Health < ped.MaxHealth;
                if (woundedPed.IsDead || woundedPed.BleedingCount > 0 || !needToHeal) continue;

                woundedPed.Health += _config.Data.WoundConfig.SelfHealingRate * Game.LastFrameTime;
                ped.Health = (int) woundedPed.Health;
            }
        }
    }
}