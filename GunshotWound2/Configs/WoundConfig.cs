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
            public readonly bool IsBlunt;
            public readonly float Damage;
            public readonly float Bleed;
            public readonly float Pain;
            public readonly bool CanCauseTrauma;

            public bool IsValid => !string.IsNullOrEmpty(Key);

            public Wound(string key,
                         string locKey,
                         bool isBlunt,
                         float damage = 0f,
                         float bleed = 0f,
                         float pain = 0f,
                         bool canCauseTrauma = false) {
                Key = key;
                LocKey = locKey;
                IsBlunt = isBlunt;
                Damage = damage;
                Bleed = bleed;
                Pain = pain;
                CanCauseTrauma = canCauseTrauma;
            }

            public Wound(XElement node) : this(key: node.GetString(nameof(Key)),
                                               locKey: node.GetString(nameof(LocKey)),
                                               isBlunt: node.GetBool(nameof(IsBlunt)),
                                               damage: node.GetFloat(nameof(Damage)),
                                               bleed: node.GetFloat(nameof(Bleed)),
                                               pain: node.GetFloat(nameof(Pain)),
                                               canCauseTrauma: node.GetBool(nameof(CanCauseTrauma))) { }

            public Wound Clone(float? damage = null, float? bleed = null, float? pain = null, bool? canCauseTrauma = null) {
                return new Wound(Key,
                                 LocKey,
                                 IsBlunt,
                                 damage ?? Damage,
                                 bleed ?? Bleed,
                                 pain ?? Pain,
                                 canCauseTrauma ?? CanCauseTrauma);
            }

            public override string ToString() {
                const string format = "F";
                string blunt = IsBlunt ? "(blunt)" : "";
                var damage = Damage.ToString(format);
                var pain = Pain.ToString(format);
                var bleed = Bleed.ToString(format);
                var trauma = CanCauseTrauma.ToString();
                return $"{Key}{blunt} D:{damage} P:{pain} B:{bleed} T:{trauma}";
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