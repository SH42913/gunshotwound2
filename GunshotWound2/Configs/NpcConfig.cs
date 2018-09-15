using System;
using GTA;

namespace GunshotWound2.Configs
{
    [Flags]
    public enum GswTargets
    {
        COMPANION = 1,
        DISLIKE = 2,
        HATE = 4,
        LIKE = 8,
        NEUTRAL = 16,
        PEDESTRIAN = 32,
        RESPECT = 64,
        ALL = 128
    }
    
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;

        public GswTargets Targets;

        public int MinAccuracy;
        public int MaxAccuracy;

        public int MinShootRate;
        public int MaxShootRate;
        
        public bool ShowEnemyCriticalMessages;
        public int MinStartHealth;
        public int MaxStartHealth;
        public float MaximalBleedStopSpeed;
        
        public float LowerMaximalPain;
        public float UpperMaximalPain;
        public float MaximalPainRecoverSpeed;

        public string NoPainAnim;
        public string MildPainAnim;
        public string AvgPainAnim;
        public string IntensePainAnim;

        public Ped[] WorldPeds;
        public int LastCheckedPedIndex;
        public int UpperBoundForFindInMs;

        public override string ToString()
        {
            return "NpcConfig:\n" +
                   $"AddingPedRange: {AddingPedRange}\n" +
                   $"RemovePedRange: {RemovePedRange}\n" +
                   $"EnemyCritical: {ShowEnemyCriticalMessages}\n" +
                   $"BleedStop: {MaximalBleedStopSpeed}\n" +
                   $"StartHealth: {MinStartHealth} - {MaxStartHealth}\n" +
                   $"MaximalPain: {LowerMaximalPain} - {UpperMaximalPain}\n" +
                   $"Accuracy: {MinAccuracy} - {MaxAccuracy}\n" +
                   $"ShootRate: {MinShootRate} - {MaxShootRate}\n" +
                   $"PainRecoverSpeed: {MaximalPainRecoverSpeed}";
        }
    }
}