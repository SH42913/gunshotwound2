// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Globalization;
    using System.Xml.Linq;
    using Utils;

    public sealed class PlayerConfig {
        public bool PoliceCanForgetYou;
        public bool PedsWillIgnoreUnconsciousPlayer;
        public bool CanDropWeapon;
        public bool InstantDeathHeadshot;
        public bool RealisticSpineDamage;

        public bool BlipsToMedkits;
        public float TimeToRefreshMedkits;
        public string MedkitModel;

        public int MoneyForHelmet;

        public float PainShockThreshold;
        public float PainRecoverSpeed;
        public float BleedHealingSpeed;
        public float PainSlowMo;
        public bool UseScreenEffects;
        public float SelfHealingRate;

        public PainMoveSets PainMoveSets;

        public void FillFrom(XDocument doc) {
            XElement node = doc.Element("Player");
            if (node == null) {
                return;
            }

            PainShockThreshold = node.Element("PainShockThreshold").GetFloat();
            PainRecoverSpeed = node.Element("PainRecoverySpeed").GetFloat();
            BleedHealingSpeed = node.Element("BleedHealSpeed").GetFloat() / 1000f;
            PoliceCanForgetYou = node.Element("PoliceCanForget").GetBool();
            PedsWillIgnoreUnconsciousPlayer = node.Element("PedsWillIgnoreUnconsciousPlayer").GetBool();
            CanDropWeapon = node.Element("CanDropWeapon").GetBool();
            InstantDeathHeadshot = node.Element("HeadshotIsInstantDeath").GetBool();
            RealisticSpineDamage = node.Element("RealisticSpineDamage").GetBool();
            BlipsToMedkits = node.Element("BlipsToMedkits").GetBool();
            TimeToRefreshMedkits = node.Element("BlipsToMedkits").GetFloat("RefreshTime");
            MedkitModel = node.Element("BlipsToMedkits").GetString("ModelName");
            PainSlowMo = node.Element("PainSlowMo").GetFloat();
            UseScreenEffects = node.Element("UseScreenEffects").GetBool();
            MoneyForHelmet = node.Element("HelmetCost").GetInt();
            SelfHealingRate = node.Element("SelfHealingRate").GetFloat();

            PainMoveSets = PainMoveSets.FromXElement(node, "MoveSets");
        }

        public override string ToString() {
            return $"{nameof(PlayerConfig)}:\n"
                   + $"{nameof(MoneyForHelmet)}: {MoneyForHelmet.ToString()}\n"
                   + $"{nameof(PainShockThreshold)}: {PainShockThreshold.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainRecoverSpeed)}: {PainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(BleedHealingSpeed)}: {BleedHealingSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(PainSlowMo)}: {PainSlowMo.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}