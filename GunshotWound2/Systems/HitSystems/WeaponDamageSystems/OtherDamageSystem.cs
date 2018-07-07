using GunshotWoundEcs.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.HitSystems.WeaponDamageSystems
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