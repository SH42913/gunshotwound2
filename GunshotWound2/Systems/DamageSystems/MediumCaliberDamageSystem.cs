using GunshotWound2.Components.HitComponents.WeaponDamageComponents;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class MediumCaliberDamageSystem : BaseGunDamageSystem<MediumCaliberHitComponent>
    {
        public MediumCaliberDamageSystem()
        {
            WeaponClass = "Medium Caliber";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 1;
            PenetratingWoundWeight = 3;
            PerforeatinWoundWeight = 5;
            AvulsiveWoundWeight = 2;
            
            DamageMultiplier = 1.3f;
            BleeedingMultiplier = 1.4f;
            PainMultiplier = 1.2f;

            HelmetSafeChance = 0.3f;
            ArmorDamage = 20;
            
            FillWithDefaultGunActions();
        }
    }
}