using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class CuttingDamageSystem : BaseWeaponDamageSystem<CuttingDamageComponent>
    {
        public CuttingDamageSystem()
        {
            WeaponClass = "Cutting";
        }

        protected override void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug("Cutting damage processing");
        }
    }
}