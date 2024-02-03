using System.Collections.Generic;
using System.Globalization;

namespace GunshotWound2.Configs {
    public sealed class WoundConfig {
        //TODO: Make it dynamic in config
        public const float MAX_SEVERITY_FOR_BANDAGE = 1f;
        private const int HEALTH_CORRECTION = 100;

        public float DamageMultiplier;
        public float PainMultiplier;
        public float BleedingMultiplier;

        public float DamageDeviation;
        public float PainDeviation;
        public float BleedingDeviation;

        public bool RealisticNervesDamage;

        public float EmergencyBleedingLevel;

        public float MoveRateOnFullPain;
        public float MoveRateOnNervesDamage;

        public bool RagdollOnPainfulWound;
        public float PainfulWoundPercent;

        public float MinimalChanceForArmorSave;

        public int BandageCost;
        public float ApplyBandageTime;
        public float SelfHealingRate;

        public Dictionary<string, float?[]> DamageSystemConfigs;

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
                   + $"{nameof(RealisticNervesDamage)}: {RealisticNervesDamage.ToString()}\n"
                   + $"D/P/B Mults: {DamageMultiplier.ToString("F2")}/{PainMultiplier.ToString("F2")}/{BleedingMultiplier.ToString("F2")}\n"
                   + $"D/P/B Deviations: {DamageDeviation.ToString("F2")}/{PainDeviation.ToString("F2")}/{BleedingDeviation.ToString("F2")}\n"
                   + $"{nameof(MoveRateOnFullPain)}: {MoveRateOnFullPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(MoveRateOnNervesDamage)}: {MoveRateOnNervesDamage.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(RagdollOnPainfulWound)}: {RagdollOnPainfulWound.ToString()}\n"
                   + $"{nameof(MinimalChanceForArmorSave)}: {MinimalChanceForArmorSave.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}