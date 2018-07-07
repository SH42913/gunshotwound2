using GunshotWound2.Components.DamageComponents.BodyDamageComponents;
using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponDamageSystems
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