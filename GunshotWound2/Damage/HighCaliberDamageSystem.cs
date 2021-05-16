using GunshotWound2.HitDetection.WeaponHitSystems;

namespace GunshotWound2.Damage
{
    public sealed class HighCaliberDamageSystem : BaseGunDamageSystem<HighCaliberHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "HighCaliber";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 1;
            PenetratingWoundWeight = 2;
            PerforatingWoundWeight = 2;
            AvulsiveWoundWeight = 4;

            DamageMultiplier = 1.3f;
            BleeedingMultiplier = 1.5f;
            PainMultiplier = 1.5f;

            HelmetSafeChance = 0.05f;
            ArmorDamage = 9;
            CanPenetrateArmor = true;
            CritChance = 0.8f;

            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}