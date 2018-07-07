using GTA.Native;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class SmallCaliberHitSystem : BaseWeaponHitSystem
    {
        public SmallCaliberHitSystem()
        {
            WeaponHashes = new []
            {
                (uint) WeaponHash.Pistol, 
                (uint) WeaponHash.CombatPistol, 
                (uint) WeaponHash.APPistol,
                (uint) WeaponHash.CombatPDW,
                (uint) WeaponHash.MachinePistol, 
                (uint) WeaponHash.MicroSMG, 
                (uint) WeaponHash.MiniSMG,
                (uint) WeaponHash.PistolMk2,
                (uint) WeaponHash.SNSPistol, 
                (uint) WeaponHash.SNSPistolMk2, 
                (uint) WeaponHash.VintagePistol,
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<SmallCaliberDamageComponent>()
                .PedEntity = pedEntity;
        }
    }
}