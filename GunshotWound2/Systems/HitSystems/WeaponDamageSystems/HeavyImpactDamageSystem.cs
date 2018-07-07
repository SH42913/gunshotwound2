using GunshotWound2.Components.DamageComponents.BodyDamageComponents;
using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponDamageSystems
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