using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class HeavyImpactDamageSystem : BaseWeaponDamageSystem<HeavyImpactDamageComponent>
    {
        public HeavyImpactDamageSystem()
        {
            WeaponClass = "HeavyImpact";
        }

        protected override void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug("Heavy impact damage processing");
        }
    }
}