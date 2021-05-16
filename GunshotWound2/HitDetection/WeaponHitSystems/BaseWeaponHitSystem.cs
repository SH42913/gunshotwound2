using GTA;
using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.Damage;
using GunshotWound2.GUI;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection.WeaponHitSystems
{
    [EcsInject]
    public sealed class BaseWeaponHitSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private readonly EcsFilter<WoundedPedComponent, HaveDamageMarkComponent> _peds = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(BaseWeaponHitSystem);
#endif

            var config = _config.Data;
            for (var i = 0; i < _peds.EntitiesCount; i++)
            {
                var ped = _peds.Components1[i].ThisPed;
                if (!ped.IsAlive) continue;

                var pedEntity = _peds.Entities[i];

                if (CheckDamageAndSendIfNeed<LightImpactHitEvent>(config.LightImpactHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<CuttingHitEvent>(config.CuttingHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<HeavyImpactHitEvent>(config.HeavyImpactHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<SmallCaliberHitEvent>(config.SmallCaliberHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<ShotgunHitEvent>(config.ShotgunHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<MediumCaliberHitEvent>(config.MediumCaliberHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<HighCaliberHitEvent>(config.HighCaliberHashes, ped, pedEntity)) continue;
                if (CheckDamageAndSendIfNeed<ExplosionHitEvent>(config.ExplosiveHashes, ped, pedEntity)) continue;

                _ecsWorld.CreateEntityWith<ShowNotificationEvent>().StringToShow = "~r~Ped damaged by unknown weapon! Please, update weapon hashes.";
            }
        }

        private bool CheckDamageAndSendIfNeed<T>(uint[] weaponHashes, Ped ped, int pedEntity) where T : BaseWeaponHitEvent, new()
        {
            if (!PedWasDamaged(weaponHashes, ped)) return false;

            _ecsWorld
                .CreateEntityWith<CheckBodyHitEvent>()
                .Entity = pedEntity;
            _ecsWorld
                .CreateEntityWith<T>()
                .Entity = pedEntity;

            return true;
        }

        private static bool PedWasDamaged(uint[] hashes, Ped target)
        {
            if (hashes == null) return false;

            foreach (var hash in hashes)
            {
                if (!Function.Call<bool>(Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, target, hash, 0)) continue;

                return true;
            }

            return false;
        }
    }
}