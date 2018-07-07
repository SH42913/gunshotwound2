using GTA.Native;
using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class CuttingHitSystem : BaseWeaponHitSystem
    {
        public CuttingHitSystem()
        {
            WeaponHashes = new uint[]
            {
                //Animal    Cougar     BarbedWire
                4194021054, 148160082, 1223143800, 
                (uint) WeaponHash.BattleAxe, 
                (uint) WeaponHash.Bottle,
                (uint) WeaponHash.Dagger,
                (uint) WeaponHash.Hatchet, 
                (uint) WeaponHash.Knife, 
                (uint) WeaponHash.Machete,
                (uint) WeaponHash.SwitchBlade,
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<CuttingDamageComponent>()
                .PedEntity = pedEntity;
        }
    }
}