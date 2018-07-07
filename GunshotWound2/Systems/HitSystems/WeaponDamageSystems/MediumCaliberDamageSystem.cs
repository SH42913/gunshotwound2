using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class MediumCaliberDamageSystem : BaseWeaponDamageSystem<MediumCaliberDamageComponent>
    {
        public MediumCaliberDamageSystem()
        {
            WeaponClass = "MediumCaliber";
        }

        protected override void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug("MediumCaliber damage processing");
        }
    }
}