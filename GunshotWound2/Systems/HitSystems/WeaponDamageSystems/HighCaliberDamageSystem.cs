using GunshotWound2.Components.DamageComponents.BodyDamageComponents;
using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;

namespace GunshotWound2.Systems.HitSystems.WeaponDamageSystems
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