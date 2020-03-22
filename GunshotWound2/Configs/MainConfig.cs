using System.Windows.Forms;

namespace GunshotWound2.Configs
{
    public class MainConfig
    {
        public WoundConfig WoundConfig;
        public NpcConfig NpcConfig;
        public PlayerConfig PlayerConfig;

        public string Language;

        public Keys? HelmetKey;
        public Keys? CheckKey;
        public Keys? HealKey;
        public Keys? IncreaseRangeKey;
        public Keys? ReduceRangeKey;
        public Keys? PauseKey;
        public Keys? BandageKey;

        public bool CommonMessages = true;
        public bool WarningMessages = true;
        public bool AlertMessages = true;
        public bool EmergencyMessages = true;

        public override string ToString()
        {
            return $"{WoundConfig}\n" +
                   $"{NpcConfig}\n" +
                   $"{PlayerConfig}";
        }

        public static void LoadDefaultValues(MainConfig config)
        {
            config.Language = "EN";

            config.HelmetKey = Keys.J;
            config.BandageKey = Keys.K;
            config.CheckKey = Keys.L;
            config.IncreaseRangeKey = Keys.PageUp;
            config.ReduceRangeKey = Keys.PageDown;
            config.PauseKey = Keys.End;
            config.HealKey = null;

            config.PlayerConfig = PlayerConfig.CreateDefault();
            config.WoundConfig = WoundConfig.CreateDefault();
            config.NpcConfig = NpcConfig.CreateDefault();
        }
    }
}