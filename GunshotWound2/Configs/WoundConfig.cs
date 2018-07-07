namespace GunshotWoundEcs.Configs
{
    public class WoundConfig
    {
        public float StopBleedingAmount;

        public float MoveRateOnFullPain;
        public float MoveRateOnNervesDamage;

        public override string ToString()
        {
            return "WoundConfig:\n" +
                   $"StopBleed: {StopBleedingAmount}\n";
        }
    }
}