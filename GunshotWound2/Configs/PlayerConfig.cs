namespace GunshotWound2.Configs
{
    public class PlayerConfig
    {
        public int PlayerEntity = -1;
        
        public bool AdrenalineSlowMotion;
        public bool WoundedPlayerEnabled;
        public bool PoliceCanForgetYou;
        public bool CanDropWeapon;
        
        public int MoneyForHelmet;
        
        public int MaximalHealth;
        public int MinimalHealth;

        public float MaximalPain;
        public float PainRecoverSpeed;
        public float BleedHealingSpeed;

        public string NoPainAnim;
        public string MildPainAnim;
        public string AvgPainAnim;
        public string IntensePainAnim;

        public bool CameraIsShaking;

        public override string ToString()
        {
            return "PlayerConfig:\n" +
                   $"WoundedPlayer: {WoundedPlayerEnabled}\n" +
                   $"HelmetCost: {MoneyForHelmet}\n" +
                   $"Min/MaxHealth: {MinimalHealth}/{MaximalHealth}\n" +
                   $"MaxPain: {MaximalPain}\n" +
                   $"PainRecoverSpeed: {PainRecoverSpeed}\n" +
                   $"BleedHealingSpeed: {BleedHealingSpeed}\n";
        }
    }
}