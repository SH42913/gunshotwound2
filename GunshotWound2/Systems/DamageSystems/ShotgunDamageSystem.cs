using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class ShotgunDamageSystem : BaseGunDamageSystem<ShotgunHitComponent>
    {
        public override void Initialize()
        {
            WeaponClass = "Shotgun";

            GrazeWoundWeight = 0;
            FleshWoundWeight = 0;
            PenetratingWoundWeight = 5;
            PerforatingWoundWeight = 0;
            AvulsiveWoundWeight = 1;
            
            DamageMultiplier = 0.7f;
            BleeedingMultiplier = 1.1f;
            PainMultiplier = 1.2f;

            HelmetSafeChance = 0.5f;
            ArmorDamage = 10;
            CritChance = 0.6f;
            
            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}