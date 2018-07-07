using System;
using GTA;

namespace GunshotWoundEcs.Components.WoundComponents
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
    
    public class WoundedPedComponent
    {
        public Ped ThisPed;
        public bool IsMale;
        public string HeShe => IsMale ? "He:" : "She";
        public bool IsPlayer;

        public int DefaultAccuracy;
        
        public float Health;
        public DamageTypes DamagedParts;
        
        public float PainMeter;
        public float MaximalPain;
        
        public int Armor;
    }
}