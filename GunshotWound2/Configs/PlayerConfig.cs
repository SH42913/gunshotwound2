// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Globalization;
    using System.Xml.Linq;
    using Utils;

    public sealed class PlayerConfig : MainConfig.IConfig {
        public bool UseVanillaHealthSystem;
        public bool PoliceCanForgetYou;
        public bool PedsWillIgnoreUnconsciousPlayer;
        public bool CanDropWeapon;
        public bool InstantDeathHeadshot;
        public bool RealisticSpineDamage;

        public int MoneyForHelmet;

        public float PainShockThreshold;
        public float PainRecoverySpeed;
        public float BleedHealingSpeed;
        public float PainSlowMo;
        public bool UseScreenEffects;
        public float SelfHealingRate;

        public string sectionName => "Player.xml";

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(PlayerConfig))!;

            UseVanillaHealthSystem = root.Element(nameof(UseVanillaHealthSystem)).GetBool();
            PainShockThreshold = root.Element(nameof(PainShockThreshold)).GetFloat();
            PainRecoverySpeed = root.Element(nameof(PainRecoverySpeed)).GetFloat();
            BleedHealingSpeed = root.Element("BleedHealSpeed").GetFloat() / 1000f;
            PoliceCanForgetYou = root.Element("PoliceCanForget").GetBool();
            PedsWillIgnoreUnconsciousPlayer = root.Element(nameof(PedsWillIgnoreUnconsciousPlayer)).GetBool();
            CanDropWeapon = root.Element(nameof(CanDropWeapon)).GetBool();
            InstantDeathHeadshot = root.Element("HeadshotIsInstantDeath").GetBool();
            RealisticSpineDamage = root.Element(nameof(RealisticSpineDamage)).GetBool();
            PainSlowMo = root.Element(nameof(PainSlowMo)).GetFloat();
            UseScreenEffects = root.Element(nameof(UseScreenEffects)).GetBool();
            MoneyForHelmet = root.Element("HelmetCost").GetInt();
            SelfHealingRate = root.Element(nameof(SelfHealingRate)).GetFloat();
        }

        public void Validate(MainConfig mainConfig, ILogger logger) { }

        public override string ToString() {
            return $"{nameof(PlayerConfig)}:\n"
                   + $"{nameof(MoneyForHelmet)}: {MoneyForHelmet.ToString()}\n"
                   + $"{nameof(PainShockThreshold)}: {PainShockThreshold.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainRecoverySpeed)}: {PainRecoverySpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(BleedHealingSpeed)}: {BleedHealingSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainSlowMo)}: {PainSlowMo.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}