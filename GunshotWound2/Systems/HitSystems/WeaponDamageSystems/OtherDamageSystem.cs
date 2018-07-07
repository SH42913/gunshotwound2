using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class OtherDamageSystem : BaseWeaponDamageSystem<OtherDamageComponent>
    {
        public OtherDamageSystem()
        {
            WeaponClass = "Other";
        }
    }
}