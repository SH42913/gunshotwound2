using System.Globalization;

namespace GunshotWound2.Configs
{
    public sealed class PlayerConfig
    {
        public int PlayerEntity = -1;

        public bool WoundedPlayerEnabled;
        public bool PoliceCanForgetYou;
        public bool CanDropWeapon;

        public int MoneyForHelmet;
        public int MinimalHealth;

        public float MaximalPain;
        public float PainRecoverSpeed;
        public float BleedHealingSpeed;
        public float MaximalSlowMo;

        public string[] MildPainSets;
        public string[] AvgPainSets;
        public string[] IntensePainSets;

        public bool CameraIsShaking;

        public int MaximalHealth => MinimalHealth + 99;

        public static PlayerConfig CreateDefault()
        {
            return new PlayerConfig
            {
                WoundedPlayerEnabled = true,
                CanDropWeapon = true,
                MoneyForHelmet = 40,
                MinimalHealth = 0,
                MaximalPain = 100,
                PainRecoverSpeed = 1.5f,
                BleedHealingSpeed = 0.001f,
                PlayerEntity = -1,
                MaximalSlowMo = 0.5f,
                PoliceCanForgetYou = true,
                MildPainSets = new[] {"move_m@injured;move_m@plodding;move_m@buzzed;"},
                AvgPainSets = new[] {"move_m@drunk@a;move_m@drunk@moderatedrunk;move_m@depressed@a;"},
                IntensePainSets = new[] {"move_m@drunk@verydrunk;move_m@strung_out@;"},
            };
        }

        public override string ToString()
        {
            return $"{nameof(PlayerConfig)}:\n" +
                   $"{nameof(WoundedPlayerEnabled)}: {WoundedPlayerEnabled.ToString()}\n" +
                   $"{nameof(MoneyForHelmet)}: {MoneyForHelmet.ToString()}\n" +
                   $"Min/MaxHealth: {MinimalHealth.ToString()}/{MaximalHealth.ToString()}\n" +
                   $"{nameof(MaximalPain)}: {MaximalPain.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(PainRecoverSpeed)}: {PainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(BleedHealingSpeed)}: {BleedHealingSpeed.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(MaximalSlowMo)}: {MaximalSlowMo.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}