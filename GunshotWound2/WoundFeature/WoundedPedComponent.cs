namespace GunshotWound2.WoundFeature {
    // TODO Check everything from here
    // public struct WoundedPedComponent : IComponent {
    //     private const int HealthOffset = 100;
    //     private const int MaxHealthOffset = HealthOffset + 1;
    //
    //     public Ped ThisPed;
    //     public bool IsPlayer;
    //     public bool IsMale;
    //
    //     public int DefaultAccuracy;
    //
    //     public float Health;
    //     public int PrevIntHealth;
    //     public bool IsDead;
    //     public CritTypes Crits;
    //
    //     public float MaximalPain;
    //     public PainStates PainState;
    //     public bool InPermanentRagdoll;
    //     public float PainRecoverSpeed;
    //
    //     public float StopBleedingAmount;
    //     public int BleedingCount;
    //     public int? MostDangerBleedingEntity;
    //
    //     public int Armor;
    //
    //     public float PedHealth {
    //         get => ThisPed.Health - HealthOffset;
    //         set {
    //             var intHealth = (int)value;
    //
    //             // if (PrevIntHealth == intHealth) return;
    //             //
    //             // PrevIntHealth = intHealth;
    //             ThisPed.Health = intHealth + HealthOffset;
    //         }
    //     }
    //
    //     public float PedMaxHealth {
    //         get => ThisPed.MaxHealth - MaxHealthOffset;
    //         set => ThisPed.MaxHealth = (int)value + MaxHealthOffset;
    //     }
    //
    //     public override string ToString() {
    //         return $"{(IsMale ? "His" : "Her")} HP:{Health}";
    //     }
    //
    //     public WoundedPedComponent CreateForNpc(Ped pedToConvert, Configs.NpcConfig npcConfig, Random random) {
    //         var woundedPed = new WoundedPedComponent();
    //         woundedPed.ThisPed = pedToConvert;
    //         woundedPed.IsMale = pedToConvert.Gender == Gender.Male;
    //         woundedPed.IsDead = false;
    //         woundedPed.IsPlayer = false;
    //         woundedPed.Armor = pedToConvert.Armor;
    //         woundedPed.Health = random.Next(npcConfig.MinStartHealth, npcConfig.MaxStartHealth);
    //         woundedPed.PedHealth = woundedPed.Health;
    //         woundedPed.PedMaxHealth = woundedPed.Health;
    //
    //         woundedPed.ThisPed.CanWrithe = false;
    //         woundedPed.ThisPed.CanWearHelmet = true;
    //         woundedPed.ThisPed.DiesOnLowHealth = false;
    //         woundedPed.ThisPed.CanSufferCriticalHits = true;
    //         woundedPed.ThisPed.CanSwitchWeapons = true;
    //         woundedPed.ThisPed.CanBeShotInVehicle = true;
    //
    //         // if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, pedToConvert)) {
    //         //     //TODO Why is it here?
    //         // }
    //
    //         woundedPed.StopBleedingAmount = random.NextFloat(
    //             npcConfig.MaximalBleedStopSpeed / 2,
    //             npcConfig.MaximalBleedStopSpeed);
    //
    //         if (npcConfig.MinAccuracy > 0 && npcConfig.MaxAccuracy > 0) {
    //             pedToConvert.Accuracy = random.Next(npcConfig.MinAccuracy, npcConfig.MaxAccuracy + 1);
    //         }
    //
    //         if (npcConfig.MinShootRate > 0 && npcConfig.MaxShootRate > 0) {
    //             pedToConvert.ShootRate = random.Next(npcConfig.MinShootRate, npcConfig.MaxShootRate);
    //         }
    //
    //         woundedPed.DefaultAccuracy = pedToConvert.Accuracy;
    //
    //         woundedPed.MaximalPain = random.NextFloat(npcConfig.LowerMaximalPain, npcConfig.UpperMaximalPain);
    //
    //         woundedPed.PainRecoverSpeed = random.NextFloat(
    //             npcConfig.MaximalPainRecoverSpeed / 2,
    //             npcConfig.MaximalPainRecoverSpeed);
    //
    //         woundedPed.Crits = 0;
    //         woundedPed.BleedingCount = 0;
    //         woundedPed.MostDangerBleedingEntity = null;
    //         return woundedPed;
    //     }
    // }
}