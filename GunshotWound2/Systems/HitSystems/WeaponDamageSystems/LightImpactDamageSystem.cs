using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class LightImpactDamageSystem : BaseWeaponDamageSystem<LightImpactDamageComponent>
    {
        public LightImpactDamageSystem()
        {
            WeaponClass = "LightImpact";
        }

        protected override void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug("LightImpact damage processing");
        }
    }
}