namespace GunshotWound2.Configs
{
    public class PlayerConfig
    {
        public bool WoundedPlayerEnabled;
        
        public int PlayerEntity = -1;
        public int MoneyForHelmet;
        
        public int MaximalHealth;
        public int MinimalHealth;

        public float MaximalPain;

        public override string ToString()
        {
            return "PlayerConfig:\n" +
                   $"WoundedPlayer: {WoundedPlayerEnabled}\n" +
                   $"HelmetCost: {MoneyForHelmet}\n" +
                   $"Min/MaxHealth: {MinimalHealth}/{MaximalHealth}\n" +
                   $"MaxPain: {MaximalPain}\n";
        }
    }
}