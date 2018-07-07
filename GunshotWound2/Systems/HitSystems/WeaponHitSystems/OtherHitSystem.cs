using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponHitSystems
{
    [EcsInject]
    public class OtherHitSystem : BaseWeaponHitSystem
    {
        public OtherHitSystem()
        {
            WeaponHashes = new uint[]
            {
                //Fall      WaterCannon Rammed     RunOverCar  HeliCrash
                3452007600, 3425972830, 133987706, 2741846334, 341774354,
            };
        }
        
        protected override void CreateComponent(int pedEntity)
        {
            EcsWorld
                .CreateEntityWith<OtherDamageComponent>()
                .PedEntity = pedEntity;
        }
    }
}