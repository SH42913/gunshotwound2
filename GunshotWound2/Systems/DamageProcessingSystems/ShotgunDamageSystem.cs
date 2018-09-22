using GunshotWound2.Components.Events.WeaponHitEvents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageProcessingSystems
{
    [EcsInject]
    public class ShotgunDamageSystem : BaseGunDamageSystem<ShotgunHitEvent>
    {
        public override void Initialize()
        {
            WeaponClass = "Shotgun";

            GrazeWoundWeight = 1;
            FleshWoundWeight = 1;
            PenetratingWoundWeight = 5;
            PerforatingWoundWeight = 0;
            AvulsiveWoundWeight = 2;
            
            DamageMultiplier = 0.8f;
            BleeedingMultiplier = 1.1f;
            PainMultiplier = 1.1f;

            HelmetSafeChance = 0.5f;
            ArmorDamage = 6;
            CritChance = 0.4f;
            CanPenetrateArmor = false;
            
            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}