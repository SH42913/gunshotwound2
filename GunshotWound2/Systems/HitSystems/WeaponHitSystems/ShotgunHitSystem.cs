using GTA.Native;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponHitSystems
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
                .CreateEntityWith<ShotgunDamageComponent>()
                .PedEntity = pedEntity;
        }
    }
}