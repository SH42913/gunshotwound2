using GTA.Native;
using GunshotWound2.Components.HitComponents.WeaponDamageComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class MediumCaliberHitSystem : BaseWeaponHitSystem, IEcsRunSystem
    {
        public MediumCaliberHitSystem()
        {
            WeaponHashes = new []
            {
                (uint) WeaponHash.AdvancedRifle, 
                (uint) WeaponHash.AssaultSMG,
                (uint) WeaponHash.BullpupRifle, 
                (uint) WeaponHash.BullpupRifleMk2, 
                (uint) WeaponHash.CarbineRifle,
                (uint) WeaponHash.CarbineRifleMk2,
                (uint) WeaponHash.CompactRifle, 
                (uint) WeaponHash.DoubleActionRevolver, 
                (uint) WeaponHash.Gusenberg,
                (uint) WeaponHash.HeavyPistol,
                (uint) WeaponHash.MarksmanPistol, 
                (uint) WeaponHash.Pistol50, 
                (uint) WeaponHash.Revolver,
                (uint) WeaponHash.RevolverMk2,
                (uint) WeaponHash.SMG, 
                (uint) WeaponHash.SMGMk2, 
                (uint) WeaponHash.SpecialCarbine,
                (uint) WeaponHash.SpecialCarbineMk2,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<MediumCaliberHitComponent>()
                .PedEntity = pedEntity;
        }
    }
}