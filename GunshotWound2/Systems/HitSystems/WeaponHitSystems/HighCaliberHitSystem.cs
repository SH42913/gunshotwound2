using GTA.Native;
using GunshotWound2.Components.HitComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class HighCaliberHitSystem : BaseWeaponHitSystem
    {
        public HighCaliberHitSystem()
        {
            WeaponHashes = new[]
            {
                (uint) WeaponHash.AssaultRifle, 
                (uint) WeaponHash.AssaultrifleMk2, 
                (uint) WeaponHash.CombatMG,
                (uint) WeaponHash.CombatMGMk2, 
                (uint) WeaponHash.HeavySniper, 
                (uint) WeaponHash.HeavySniperMk2,
                (uint) WeaponHash.MarksmanRifle, 
                (uint) WeaponHash.MarksmanRifleMk2, 
                (uint) WeaponHash.MG,
                (uint) WeaponHash.Minigun,
                (uint) WeaponHash.Musket, 
                (uint) WeaponHash.Railgun,
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<HighCaliberHitComponent>()
                .PedEntity = pedEntity;
        }
    }
}