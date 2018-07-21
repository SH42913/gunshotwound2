using GunshotWound2.Components.HitComponents.WeaponDamageComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class SmallCaliberDamageSystem : BaseGunDamageSystem<SmallCaliberHitComponent>
    {
        public SmallCaliberDamageSystem()
        {
            WeaponClass = "Small Caliber";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 1;
            PenetratingWoundWeight = 4;
            PerforeatinWoundWeight = 0;
            AvulsiveWoundWeight = 2;
            
            DamageMultiplier = 1f;
            BleeedingMultiplier = 1f;
            PainMultiplier = 1f;

            HelmetSafeChance = 0.7f;
            ArmorDamage = 10;
            CanPenetrateArmor = true;

            FillWithDefaultGunActions();
        }
    }
}