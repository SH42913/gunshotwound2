using GTA.Native;
using GunshotWound2.Components.Events.WeaponHitEvents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class HighCaliberHitSystem : BaseWeaponHitSystem
    {
        protected override uint[] GetWeaponHashes()
        {
            return new[]
            { 
                (uint) WeaponHash.Pistol50, 
                (uint) WeaponHash.MG,
                (uint) WeaponHash.CombatMGMk2,
                (uint) WeaponHash.AssaultRifle, 
                (uint) WeaponHash.HeavySniper, 
                (uint) WeaponHash.HeavySniperMk2,
                (uint) WeaponHash.MarksmanRifle, 
                (uint) WeaponHash.MarksmanRifleMk2, 
                (uint) WeaponHash.Minigun,
                (uint) WeaponHash.Railgun,
                (uint) WeaponHash.Musket, 
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<HighCaliberHitEvent>()
                .PedEntity = pedEntity;
        }
    }
}