namespace GunshotWound2.Configs
{
    public class MainConfig
    {
        public int TicksToRefresh;

        public WoundConfig WoundConfig;
        public NpcConfig NpcConfig;
        public PlayerConfig PlayerConfig;

        public override string ToString()
        {
            return $"{WoundConfig}\n" +
                   $"{NpcConfig}\n" +
                   $"{PlayerConfig}";
        }
    }
}