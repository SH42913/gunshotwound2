namespace GunshotWound2.Configs {
    using System.Globalization;

    public sealed class PlayerConfig {
        public bool PoliceCanForgetYou;
        public bool CanDropWeapon;

        public bool BlipsToMedkits;
        public float TimeToRefreshMedkits;
        public string MedkitModel;

        public int MoneyForHelmet;

        public float MaximalPain;
        public float PainRecoverSpeed;
        public float BleedHealingSpeed;
        public float PainSlowMo;

        public PainMoveSets PainMoveSets;

        public override string ToString() {
            return $"{nameof(PlayerConfig)}:\n"
                   + $"{nameof(MoneyForHelmet)}: {MoneyForHelmet.ToString()}\n"
                   + $"{nameof(MaximalPain)}: {MaximalPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainRecoverSpeed)}: {PainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(BleedHealingSpeed)}: {BleedHealingSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainSlowMo)}: {PainSlowMo.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}