using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class SmallCaliberDamageSystem : BaseGunDamageSystem<SmallCaliberHitComponent>
    {
        public override void Initialize()
        {
            WeaponClass = "Small Caliber";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 2;
            PenetratingWoundWeight = 6;
            PerforeatinWoundWeight = 0;
            AvulsiveWoundWeight = 2;
            
            DamageMultiplier = 1f;
            BleeedingMultiplier = 1f;
            PainMultiplier = 1f;

            HelmetSafeChance = 0.7f;
            ArmorDamage = 5;
            CanPenetrateArmor = true;
            
            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}