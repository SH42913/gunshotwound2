// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Utils;

    public sealed class WoundConfig {
        public readonly struct Wound {
            public readonly string Key;
            public readonly string LocKey;
            public readonly float Damage;
            public readonly float Bleed;
            public readonly float Pain;
            public readonly float ArteryChance;
            public readonly bool IsInternal;
            public readonly bool WithCrit;
            public readonly bool ForceCrit;

            public Wound(string key,
                            string locKey,
                            float damage,
                            float bleed,
                            float pain,
                            float arteryChance,
                            bool isInternal,
                            bool withCrit,
                            bool forceCrit) {
                Key = key;
                LocKey = locKey;
                Damage = damage;
                Bleed = bleed;
                Pain = pain;
                ArteryChance = arteryChance;
                IsInternal = isInternal;
                WithCrit = withCrit;
                ForceCrit = forceCrit;
            }

            public Wound(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                Damage = node.GetFloat(nameof(Damage));
                Bleed = node.GetFloat(nameof(Bleed));
                Pain = node.GetFloat(nameof(Pain));
                ArteryChance = node.GetFloat(nameof(ArteryChance));
                IsInternal = node.GetBool(nameof(IsInternal));
                WithCrit = node.GetBool(nameof(WithCrit));
                ForceCrit = node.GetBool(nameof(ForceCrit));
            }
        }

        public const float MAX_SEVERITY_FOR_BANDAGE = 1f;
        private const int HEALTH_CORRECTION = 100;

        public float OverallDamageMult;
        public float OverallPainMult;
        public float OverallBleedingMult;

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

        public Dictionary<string, Wound> Wounds;

        public void FillFrom(XDocument doc) {
            XElement node = doc.Element(nameof(WoundConfig))!;

            MoveRateOnFullPain = node.Element(nameof(MoveRateOnFullPain)).GetFloat();
            MoveRateOnLegsCrit = node.Element(nameof(MoveRateOnLegsCrit)).GetFloat();
            OverallDamageMult = node.Element(nameof(OverallDamageMult)).GetFloat();
            DamageDeviation = node.Element(nameof(DamageDeviation)).GetFloat();
            OverallPainMult = node.Element(nameof(OverallPainMult)).GetFloat();
            PainDeviation = node.Element(nameof(PainDeviation)).GetFloat();
            OverallBleedingMult = node.Element(nameof(OverallBleedingMult)).GetFloat();
            BleedingDeviation = node.Element(nameof(BleedingDeviation)).GetFloat();
            RagdollOnPainfulWound = node.Element(nameof(RagdollOnPainfulWound)).GetBool();
            PainfulWoundPercent = node.Element(nameof(PainfulWoundPercent)).GetFloat();
            UseCustomUnconsciousBehaviour = node.Element(nameof(UseCustomUnconsciousBehaviour)).GetBool();
            DelayedPainPercent = node.Element(nameof(DelayedPainPercent)).GetFloat();
            DeadlyPainShockPercent = node.Element(nameof(DeadlyPainShockPercent)).GetFloat();

            XElement itemsNode = node.Element(nameof(Wounds))!;
            Wounds = itemsNode.Elements(nameof(Wound)).Select(x => new Wound(x)).ToDictionary(x => x.Key);
        }

        public bool IsBleedingCanBeBandaged(float severity) {
            return severity <= OverallBleedingMult * MAX_SEVERITY_FOR_BANDAGE;
        }

        public static int ConvertHealthFromNative(int health) {
            return health - HEALTH_CORRECTION;
        }

        public static int ConvertHealthToNative(int health) {
            return health + HEALTH_CORRECTION;
        }

        public override string ToString() {
            return $"{nameof(WoundConfig)}:\n"
                   + $"D/P/B Mults: {OverallDamageMult.ToString("F2")}/{OverallPainMult.ToString("F2")}/{OverallBleedingMult.ToString("F2")}\n"
                   + $"D/P/B Deviations: {DamageDeviation.ToString("F2")}/{PainDeviation.ToString("F2")}/{BleedingDeviation.ToString("F2")}\n"
                   + $"{nameof(MoveRateOnFullPain)}: {MoveRateOnFullPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(MoveRateOnLegsCrit)}: {MoveRateOnLegsCrit.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(RagdollOnPainfulWound)}: {RagdollOnPainfulWound.ToString()}";
        }
    }
}