namespace GunshotWoundEcs.Configs
{
    public class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;
        public bool ShowEnemyCriticalMessages;

        public override string ToString()
        {
            return "NpcConfig:\n" +
                   $"AddingPedRange: {AddingPedRange}\n" +
                   $"RemovePedRange: {RemovePedRange}\n";
        }
    }
}