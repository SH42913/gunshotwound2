using GunshotWoundEcs.Components.DamageComponents.BodyDamageComponents;
using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
{
    public class HighCaliberDamageSystem : BaseWeaponDamageSystem<HighCaliberDamageComponent>
    {
        public HighCaliberDamageSystem()
        {
            WeaponClass = "HighCaliber";
        }

        protected override void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug("HighCaliber damage processing");
        }
    }
}