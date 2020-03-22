using GTA.Native;
using Leopotam.Ecs;

namespace GunshotWound2.HitDetection.WeaponHitSystems
{
    [EcsInject]
    public class MediumCaliberHitSystem : BaseWeaponHitSystem, IEcsRunSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new[]
            {
                (uint) WeaponHash.HeavyPistol,
                (uint) WeaponHash.Revolver,
                (uint) WeaponHash.RevolverMk2,
                (uint) WeaponHash.AssaultrifleMk2,
                (uint) WeaponHash.AdvancedRifle,
                (uint) WeaponHash.AssaultSMG,
                (uint) WeaponHash.BullpupRifle,
                (uint) WeaponHash.BullpupRifleMk2,
                (uint) WeaponHash.CarbineRifle,
                (uint) WeaponHash.CarbineRifleMk2,
                (uint) WeaponHash.CombatMG,
                (uint) WeaponHash.CompactRifle,
                (uint) WeaponHash.Gusenberg,
                (uint) WeaponHash.SpecialCarbine,
                (uint) WeaponHash.SpecialCarbineMk2,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<MediumCaliberHitEvent>()
                .Entity = pedEntity;
        }
    }
}