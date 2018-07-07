namespace GunshotWound2.Configs
{
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;
        public bool ShowEnemyCriticalMessages;
        public float MaximalHealth;

        public override string ToString()
        {
            return "NpcConfig:\n" +
                   $"AddingPedRange: {AddingPedRange}\n" +
                   $"RemovePedRange: {RemovePedRange}\n" +
                   $"EnemyCritical: {ShowEnemyCriticalMessages}\n" +
                   $"MaximalHealth: {MaximalHealth}\n";
        }
    }
}