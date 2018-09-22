using GTA.Native;
using GunshotWound2.Components.Events.WeaponHitEvents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class ExplosionHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return  new uint[]
            {
                //Explosion
                539292904, 
                (uint) WeaponHash.Grenade, 
                (uint) WeaponHash.CompactGrenadeLauncher,
                (uint) WeaponHash.GrenadeLauncher, 
                (uint) WeaponHash.HomingLauncher, 
                (uint) WeaponHash.PipeBomb,
                (uint) WeaponHash.ProximityMine, 
                (uint) WeaponHash.RPG, 
                (uint) WeaponHash.StickyBomb,
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<ExplosionHitEvent>()
                .Entity = pedEntity;
        }
    }
}