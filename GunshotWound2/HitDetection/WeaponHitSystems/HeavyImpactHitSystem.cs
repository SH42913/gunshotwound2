using GTA.Native;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection.WeaponHitSystems
{
    [EcsInject]
    public class HeavyImpactHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new uint[]
            {
                //Rammed     RunOverCar  HeliCrash
                133987706, 2741846334, 341774354,
                (uint) WeaponHash.Bat,
                (uint) WeaponHash.Crowbar,
                (uint) WeaponHash.FireExtinguisher,
                (uint) WeaponHash.Firework,
                (uint) WeaponHash.GolfClub,
                (uint) WeaponHash.Hammer,
                (uint) WeaponHash.PetrolCan,
                (uint) WeaponHash.PoolCue,
                (uint) WeaponHash.Wrench,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<HeavyImpactHitEvent>()
                .Entity = pedEntity;
        }
    }
}