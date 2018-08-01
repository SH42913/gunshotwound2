using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public class ShotgunDamageSystem : BaseGunDamageSystem<ShotgunHitComponent>
    {
        public ShotgunDamageSystem()
        {
            WeaponClass = "Shotgun";

            GrazeWoundWeight = 0;
            FleshWoundWeight = 0;
            PenetratingWoundWeight = 5;
            PerforeatinWoundWeight = 0;
            AvulsiveWoundWeight = 1;
            
            DamageMultiplier = 0.7f;
            BleeedingMultiplier = 1.1f;
            PainMultiplier = 1.2f;

            HelmetSafeChance = 0.5f;
            ArmorDamage = 10;
            
            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}