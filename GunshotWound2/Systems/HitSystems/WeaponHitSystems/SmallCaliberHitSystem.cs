using GTA.Native;
using GunshotWound2.Components.Events.WeaponHitEvents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class SmallCaliberHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new []
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
                .CreateEntityWith<SmallCaliberHitEvent>()
                .PedEntity = pedEntity;
        }
    }
}