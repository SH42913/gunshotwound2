using System;
using GTA;

namespace GunshotWound2.HitDetection
{
    [Flags]
    public enum CritTypes
    {
        LEGS_DAMAGED = 1 << 0,
        ARMS_DAMAGED = 1 << 1,
        NERVES_DAMAGED = 1 << 2,
        GUTS_DAMAGED = 1 << 3,
        STOMACH_DAMAGED = 1 << 4,
        LUNGS_DAMAGED = 1 << 5,
        HEART_DAMAGED = 1 << 6,
    }

    public enum PainStates
    {
        NONE,
        MILD,
        AVERAGE,
        INTENSE,
        UNBEARABLE,
        DEADLY
    }

    public sealed class WoundedPedComponent
    {
        private const int HealthOffset = 100;
        private const int MaxHealthOffset = HealthOffset + 1;

        public Ped ThisPed;
        public bool IsPlayer;
        public bool IsMale;

        public int DefaultAccuracy;

        public float Health;
        public int PrevIntHealth;
        public bool IsDead;
        public CritTypes Crits;

        public float MaximalPain;
        public PainStates PainState;
        public bool InPermanentRagdoll;
        public float PainRecoverSpeed;

        public float StopBleedingAmount;
        public int BleedingCount;
        public int? MostDangerBleedingEntity;

        public int Armor;

        public float PedHealth
        {
            get => ThisPed.Health - HealthOffset;
            set
            {
                var intHealth = (int) value;
                // if (PrevIntHealth == intHealth) return;
                //
                // PrevIntHealth = intHealth;
                ThisPed.Health = intHealth + HealthOffset;
            }
        }

        public float PedMaxHealth
        {
            get => ThisPed.MaxHealth - MaxHealthOffset;
            set => ThisPed.MaxHealth = (int) value + MaxHealthOffset;
        }

        public override string ToString()
        {
            return $"{(IsMale ? "His" : "Her")} HP:{Health}";
        }
    }

    public static class CritTypesExtension
    {
        public static bool Has(this CritTypes category, CritTypes value)
        {
            return (category & value) == value;
        }
    }
}