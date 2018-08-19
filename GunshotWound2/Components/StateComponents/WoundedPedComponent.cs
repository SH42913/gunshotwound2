using System;
using GTA;

namespace GunshotWound2.Components.StateComponents
{
    [Flags]
    public enum DamageTypes
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
        public string HeShe;
        public string HisHer;

        public int DefaultAccuracy;
        
        public float Health;
        public bool IsDead;
        public DamageTypes DamagedParts;
        public float StopBleedingAmount;
        
        public float PainMeter;
        public float MaximalPain;
        public PainStates PainState;
        public bool GivesInToPain;
        public float PainRecoverSpeed;
        
        public int Armor;

        public override string ToString()
        {
            return $"{HisHer} HP:{Health} Pain:{PainMeter / MaximalPain * 100:0.0}%";
        }
    }
}