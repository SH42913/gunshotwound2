namespace GunshotWound2.Configs
{
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;
        
        public bool ShowEnemyCriticalMessages;
        public int MinimalStartHealth;
        public int MaximalStartHealth;
        public float MaximalBleedStopSpeed;
        
        public float MaximalPain;
        public float MaximalPainRecoverSpeed;

        public string NoPainAnim;
        public string MildPainAnim;
        public string AvgPainAnim;
        public string IntensePainAnim;

        public override string ToString()
        {
            return "NpcConfig:\n" +
                   $"AddingPedRange: {AddingPedRange}\n" +
                   $"RemovePedRange: {RemovePedRange}\n" +
                   $"EnemyCritical: {ShowEnemyCriticalMessages}\n" +
                   $"BleedStop: {MaximalBleedStopSpeed}\n" +
                   $"MaximalStartHealth: {MaximalStartHealth}\n" +
                   $"MaximalPain: {MaximalPain}\n" +
                   $"PainRecoverSpeed: {MaximalPainRecoverSpeed}\n";
        }
    }
}