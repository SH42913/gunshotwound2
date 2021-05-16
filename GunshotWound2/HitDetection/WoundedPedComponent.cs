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
        public Ped ThisPed;
        public bool IsPlayer;
        public bool IsMale;

        public int DefaultAccuracy;

        public float Health;
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