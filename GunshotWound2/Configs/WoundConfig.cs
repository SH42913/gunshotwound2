﻿using System.Collections.Generic;
using System.Globalization;

namespace GunshotWound2.Configs
{
    public sealed class WoundConfig
    {
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
        public float PainfulWoundValue;

        public float MinimalChanceForArmorSave;

        public int BandageCost;
        public float ApplyBandageTime;
        public float SelfHealingRate;

        public Dictionary<string, float?[]> DamageSystemConfigs;

        public static WoundConfig CreateDefault()
        {
            return new WoundConfig
            {
                MoveRateOnNervesDamage = 0.7f,
                MoveRateOnFullPain = 0.8f,
                EmergencyBleedingLevel = 1.5f,
                RealisticNervesDamage = true,
                DamageMultiplier = 1,
                BleedingMultiplier = 1,
                PainMultiplier = 1,
                DamageDeviation = 0.2f,
                BleedingDeviation = 0.2f,
                PainDeviation = 0.2f,
                RagdollOnPainfulWound = true,
                PainfulWoundValue = 50,
                MinimalChanceForArmorSave = 0.6f,
                BandageCost = 15,
                ApplyBandageTime = 5,
                SelfHealingRate = 0.01f
            };
        }

        public override string ToString()
        {
            return $"{nameof(WoundConfig)}:\n" +
                   $"{nameof(RealisticNervesDamage)}: {RealisticNervesDamage.ToString()}\n" +
                   $"D/P/B Mults: {DamageMultiplier.ToString()}/{PainMultiplier.ToString()}/{BleedingMultiplier.ToString()}\n" +
                   $"D/P/B Deviations: {DamageDeviation.ToString()}/{PainDeviation.ToString()}/{BleedingDeviation.ToString()}\n" +
                   $"{nameof(MoveRateOnFullPain)}: {MoveRateOnFullPain.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(MoveRateOnNervesDamage)}: {MoveRateOnNervesDamage.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(RagdollOnPainfulWound)}: {RagdollOnPainfulWound.ToString()}\n" +
                   $"{nameof(PainfulWoundValue)}: {PainfulWoundValue.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(MinimalChanceForArmorSave)}: {MinimalChanceForArmorSave.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}