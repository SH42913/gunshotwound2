using GTA.Native;
using GunshotWound2.Components.HitComponents.WeaponDamageComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class ShotgunHitSystem : BaseWeaponHitSystem
    {
        public ShotgunHitSystem()
        {
            WeaponHashes = new[]
            {
                (uint) WeaponHash.AssaultShotgun,
                (uint) WeaponHash.BullpupShotgun,
                (uint) WeaponHash.DoubleBarrelShotgun,
                (uint) WeaponHash.HeavyShotgun,
                (uint) WeaponHash.PumpShotgun,
                (uint) WeaponHash.PumpShotgunMk2,
                (uint) WeaponHash.SawnOffShotgun,
                (uint) WeaponHash.SweeperShotgun,
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<ShotgunHitComponent>()
                .PedEntity = pedEntity;
        }
    }
}