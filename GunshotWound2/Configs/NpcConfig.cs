using GTA;

namespace GunshotWound2.Configs
{
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;

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