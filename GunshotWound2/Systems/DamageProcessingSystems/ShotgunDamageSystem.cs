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

            GrazeWoundWeight = 0;
            FleshWoundWeight = 0;
            PenetratingWoundWeight = 5;
            PerforatingWoundWeight = 0;
            AvulsiveWoundWeight = 1;
            
            DamageMultiplier = 0.7f;
            BleeedingMultiplier = 1.1f;
            PainMultiplier = 1.2f;

            HelmetSafeChance = 0.5f;
            ArmorDamage = 6;
            CritChance = 0.6f;
            
            LoadMultsFromConfig();
            FillWithDefaultGunActions();
        }
    }
}