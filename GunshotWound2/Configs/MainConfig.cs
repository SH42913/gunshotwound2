namespace GunshotWoundEcs.Configs
{
    public class MainConfig
    {
        public bool Debug;
        public int TicksToRefresh;

        public override string ToString()
        {
            return "MainConfig:\n" +
                   $"Debug: {Debug}\n" +
                   $"TicksToRefresh: {TicksToRefresh}\n";
        }
    }
}