using System;
using GTA;

namespace GunshotWound2.Components.StateComponents
{
    [Flags]
    public enum CritTypes
    {
        LEGS_DAMAGED = 1,
        ARMS_DAMAGED = 2,
        NERVES_DAMAGED = 4,
        GUTS_DAMAGED = 8,
        STOMACH_DAMAGED = 16,
        LUNGS_DAMAGED = 32,
        HEART_DAMAGED = 64,
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
    
    public class WoundedPedComponent
    {
        public Ped ThisPed;
        public bool IsPlayer;
        public bool IsMale;

        public int DefaultAccuracy;
        
        public float Health;
        public bool IsDead;
        public CritTypes Crits;
        public float StopBleedingAmount;
        
        public float MaximalPain;
        public PainStates PainState;
        public bool InPermanentRagdoll;
        public float PainRecoverSpeed;

        public int WoundCount;
        
        public int Armor;

        public override string ToString()
        {
            return $"{(IsMale ? "His" : "Her")} HP:{Health}";
        }
    }
}