using GTA;

namespace GunshotWound2.Configs
{
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;
        
        public bool ShowEnemyCriticalMessages;
        public int LowerStartHealth;
        public int UpperStartHealth;
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
                   $"StartHealth: {LowerStartHealth} - {UpperStartHealth}\n" +
                   $"MaximalPain: {LowerMaximalPain} - {UpperMaximalPain}\n" +
                   $"PainRecoverSpeed: {MaximalPainRecoverSpeed}\n";
        }
    }
}