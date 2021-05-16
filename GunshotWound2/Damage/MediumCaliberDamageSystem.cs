using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public sealed class MediumCaliberDamageSystem : BaseGunDamageSystem<MediumCaliberHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "MediumCaliber";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 2;
            PenetratingWoundWeight = 5;
            PerforatingWoundWeight = 6;
            AvulsiveWoundWeight = 1;

            DamageMultiplier = 1.2f;
            BleeedingMultiplier = 1.3f;
            PainMultiplier = 1.2f;

            HelmetSafeChance = 0.3f;
            ArmorDamage = 6;
            CanPenetrateArmor = true;
            CritChance = 0.6f;

            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}