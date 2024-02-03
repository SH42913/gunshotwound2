using System.Globalization;

namespace GunshotWound2.Configs {
    public sealed class PlayerConfig {
        public bool WoundedPlayerEnabled;
        public bool PoliceCanForgetYou;
        public bool CanDropWeapon;

        public int MoneyForHelmet;

        public float MaximalPain;
        public float PainRecoverSpeed;
        public float BleedHealingSpeed;
        public float MaximalSlowMo;

        public string[] MildPainSets;
        public string[] AvgPainSets;
        public string[] IntensePainSets;

        public override string ToString() {
            return $"{nameof(PlayerConfig)}:\n"
                   + $"{nameof(WoundedPlayerEnabled)}: {WoundedPlayerEnabled.ToString()}\n"
                   + $"{nameof(MoneyForHelmet)}: {MoneyForHelmet.ToString()}\n"
                   + $"{nameof(MaximalPain)}: {MaximalPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainRecoverSpeed)}: {PainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(BleedHealingSpeed)}: {BleedHealingSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(MaximalSlowMo)}: {MaximalSlowMo.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}