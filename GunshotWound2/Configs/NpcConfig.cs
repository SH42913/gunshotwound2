namespace GunshotWound2.Configs
{
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;
        public bool ShowEnemyCriticalMessages;
        public int MaximalHealth;
        public float MaximalBleedStopSpeed;
        public float MaximalPain;
        public float MaximalPainRecoverSpeed;

        public override string ToString()
        {
            return "NpcConfig:\n" +
                   $"AddingPedRange: {AddingPedRange}\n" +
                   $"RemovePedRange: {RemovePedRange}\n" +
                   $"EnemyCritical: {ShowEnemyCriticalMessages}" +
                   $"BleedStop: {MaximalBleedStopSpeed}\n" +
                   $"MaximalHealth: {MaximalHealth}\n" +
                   $"MaximalPain: {MaximalPain}\n" +
                   $"PainRecoverSpeed: {MaximalPainRecoverSpeed}\n";
        }
    }
}