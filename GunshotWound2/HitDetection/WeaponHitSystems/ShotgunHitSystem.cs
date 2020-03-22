using GTA.Native;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection.WeaponHitSystems
{
    [EcsInject]
    public sealed class ShotgunHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new[]
            {
                (uint) WeaponHash.PumpShotgun,
                (uint) WeaponHash.PumpShotgunMk2,
                (uint) WeaponHash.SawnOffShotgun,
                (uint) WeaponHash.BullpupShotgun,
                (uint) WeaponHash.AssaultShotgun,
                (uint) WeaponHash.HeavyShotgun,
                (uint) WeaponHash.DoubleBarrelShotgun,
                (uint) WeaponHash.SweeperShotgun,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<ShotgunHitEvent>()
                .Entity = pedEntity;
        }
    }
}