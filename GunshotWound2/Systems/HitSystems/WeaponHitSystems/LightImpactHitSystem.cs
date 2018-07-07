using GTA.Native;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponHitSystems
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
                .CreateEntityWith<LightImpactDamageComponent>()
                .PedEntity = pedEntity;
        }
    }
}