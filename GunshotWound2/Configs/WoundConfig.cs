// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Globalization;
    using System.Xml.Linq;
    using Utils;

    public sealed class WoundConfig {
        public const float MAX_SEVERITY_FOR_BANDAGE = 1f;
        private const int HEALTH_CORRECTION = 100;

        public float DamageMultiplier;
        public float PainMultiplier;
        public float BleedingMultiplier;

        public float DamageDeviation;
        public float PainDeviation;
        public float BleedingDeviation;

        public float MoveRateOnFullPain;
        public float MoveRateOnLegsCrit;

        public bool RagdollOnPainfulWound;
        public float PainfulWoundPercent;
        public bool UseCustomUnconsciousBehaviour;
        public float DelayedPainPercent;
        public float DeadlyPainShockPercent;

        public float SelfHealingRate;

        public void FillFrom(XElement doc) {
            XElement node = doc.Element("Wounds");
            if (node == null) {
                return;
            }

            MoveRateOnFullPain = node.Element("MoveRateOnFullPain").GetFloat();
            MoveRateOnLegsCrit = node.Element("MoveRateOnLegsCrit").GetFloat();
            DamageMultiplier = node.Element("OverallDamageMult").GetFloat();
            DamageDeviation = node.Element("DamageDeviation").GetFloat();
            PainMultiplier = node.Element("OverallPainMult").GetFloat();
            PainDeviation = node.Element("PainDeviation").GetFloat();
            BleedingMultiplier = node.Element("OverallBleedingMult").GetFloat();
            BleedingDeviation = node.Element("BleedingDeviation").GetFloat();
            RagdollOnPainfulWound = node.Element("RagdollOnPainfulWound").GetBool();
            PainfulWoundPercent = node.Element("PainfulWoundPercent").GetFloat();
            UseCustomUnconsciousBehaviour = node.Element("UseCustomUnconsciousBehaviour").GetBool();
            DelayedPainPercent = node.Element("DelayedPainPercent").GetFloat();
            DeadlyPainShockPercent = node.Element("DeadlyPainShockPercent").GetFloat();
            SelfHealingRate = node.Element("SelfHealingRate").GetFloat();
        }

        public bool IsBleedingCanBeBandaged(float severity) {
            return severity <= BleedingMultiplier * MAX_SEVERITY_FOR_BANDAGE;
        }

        public static int ConvertHealthFromNative(int health) {
            return health - HEALTH_CORRECTION;
        }

        public static int ConvertHealthToNative(int health) {
            return health + HEALTH_CORRECTION;
        }

        public override string ToString() {
            return $"{nameof(WoundConfig)}:\n"
                   + $"D/P/B Mults: {DamageMultiplier.ToString("F2")}/{PainMultiplier.ToString("F2")}/{BleedingMultiplier.ToString("F2")}\n"
                   + $"D/P/B Deviations: {DamageDeviation.ToString("F2")}/{PainDeviation.ToString("F2")}/{BleedingDeviation.ToString("F2")}\n"
                   + $"{nameof(MoveRateOnFullPain)}: {MoveRateOnFullPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(MoveRateOnLegsCrit)}: {MoveRateOnLegsCrit.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(RagdollOnPainfulWound)}: {RagdollOnPainfulWound.ToString()}";
        }
    }
}