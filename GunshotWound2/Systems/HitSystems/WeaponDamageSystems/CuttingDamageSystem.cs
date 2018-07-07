using GunshotWound2.Components.DamageComponents.BodyDamageComponents;
using GunshotWound2.Components.DamageComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.HitSystems.WeaponDamageSystems
{
    [EcsInject]
    public class CuttingDamageSystem : BaseWeaponDamageSystem<CuttingDamageComponent>
    {
        public CuttingDamageSystem()
        {
            WeaponClass = "Cutting";
        }

        protected override void ProcessWound(BodyDamageComponent bodyDamage, int pedEntity)
        {
            SendDebug("Cutting damage processing");
        }
    }
}