using GunshotWound2.Components.DamageComponents.BodyDamageComponents;
using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponDamageSystems
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