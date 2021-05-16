using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public sealed class SmallCaliberDamageSystem : BaseGunDamageSystem<SmallCaliberHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "SmallCaliber";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 2;
            PenetratingWoundWeight = 6;
            PerforatingWoundWeight = 1;
            AvulsiveWoundWeight = 1;

            HelmetSafeChance = 0.8f;
            ArmorDamage = 3;
            CanPenetrateArmor = true;
            CritChance = 0.4f;

            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}