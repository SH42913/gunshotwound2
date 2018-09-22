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
                (uint) WeaponHash.PistolMk2,
                (uint) WeaponHash.CombatPistol, 
                (uint) WeaponHash.SNSPistol, 
                (uint) WeaponHash.SNSPistolMk2,
                (uint) WeaponHash.VintagePistol,
                (uint) WeaponHash.MarksmanPistol,
                (uint) WeaponHash.DoubleActionRevolver, 
                (uint) WeaponHash.APPistol,
                (uint) WeaponHash.MicroSMG, 
                (uint) WeaponHash.MiniSMG,
                (uint) WeaponHash.SMG, 
                (uint) WeaponHash.SMGMk2, 
                (uint) WeaponHash.CombatPDW,
                (uint) WeaponHash.MachinePistol,  
            };
        }

        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<SmallCaliberHitEvent>()
                .Entity = pedEntity;
        }
    }
}