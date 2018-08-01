using GTA.Native;
using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class LightImpactHitSystem : BaseWeaponHitSystem
    {
        public LightImpactHitSystem()
        {
            WeaponHashes = new uint[]
            {
                //GarbageBug Briefcase  Briefcase2
                3794977420, 2294779575, 28811031,
                //Fall      WaterCannon
                3452007600, 3425972830, 
                (uint) WeaponHash.Ball, 
                (uint) WeaponHash.Flashlight, 
                (uint) WeaponHash.KnuckleDuster,
                (uint) WeaponHash.Nightstick, 
                (uint) WeaponHash.Snowball,
                (uint) WeaponHash.Unarmed, 
                (uint) WeaponHash.Parachute,
                (uint) WeaponHash.NightVision,
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<LightImpactHitComponent>()
                .PedEntity = pedEntity;
        }
    }
}