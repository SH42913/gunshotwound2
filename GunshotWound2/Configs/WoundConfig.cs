// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using Utils;

    public sealed class WoundConfig : MainConfig.IConfig {
        public readonly struct Wound {
            public readonly string Key;
            public readonly string LocKey;
            public readonly bool IsBlunt;
            public readonly DBPContainer DBP;
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
                DBP = new DBPContainer(damage, bleed, pain);
                CanCauseTrauma = canCauseTrauma;
            }

            public Wound(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                IsBlunt = node.GetBool(nameof(IsBlunt));
                DBP = new DBPContainer(node, isMult: false);
                CanCauseTrauma = node.GetBool(nameof(CanCauseTrauma));
            }

            public Wound Clone(float? damage = null, float? bleed = null, float? pain = null, bool? canCauseTrauma = null) {
                return new Wound(Key,
                                 LocKey,
                                 IsBlunt,
                                 damage ?? DBP.damage,
                                 bleed ?? DBP.bleed,
                                 pain ?? DBP.pain,
                                 canCauseTrauma ?? CanCauseTrauma);
            }

            public override string ToString() {
                string blunt = IsBlunt ? "(blunt)" : "";
                return $"{Key}{blunt} {DBP.ToString()} T:{CanCauseTrauma.ToString()}";
            }
        }

        public const float MAX_SEVERITY_FOR_BANDAGE = 1f;
        private const int HEALTH_CORRECTION = 100;

        public DBPContainer GlobalMultipliers;
        public DBPContainer GlobalDeviations;

        public float MoveRateOnFullPain;
        public float MoveRateOnLegsTrauma;

        public bool RagdollOnPainfulWound;
        public float PainfulWoundPercent;
        public bool UseCustomUnconsciousBehaviour;
        public float DelayedPainPercent;
        public float DeadlyPainShockPercent;

        public int TakedownRagdollDurationMs;
        public DBPContainer TakedownMults;

        public Dictionary<string, Wound> Wounds;

        public string sectionName => "Wounds.xml";

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(WoundConfig))!;

            GlobalMultipliers = new DBPContainer(root.Element(nameof(GlobalMultipliers)), isMult: true);
            GlobalDeviations = new DBPContainer(root.Element(nameof(GlobalDeviations)), isMult: false);

            MoveRateOnFullPain = root.Element(nameof(MoveRateOnFullPain)).GetFloat();
            MoveRateOnLegsTrauma = root.Element(nameof(MoveRateOnLegsTrauma)).GetFloat();
            RagdollOnPainfulWound = root.Element(nameof(RagdollOnPainfulWound)).GetBool();
            PainfulWoundPercent = root.Element(nameof(PainfulWoundPercent)).GetFloat();
            UseCustomUnconsciousBehaviour = root.Element(nameof(UseCustomUnconsciousBehaviour)).GetBool();
            DelayedPainPercent = root.Element(nameof(DelayedPainPercent)).GetFloat();
            DeadlyPainShockPercent = root.Element(nameof(DeadlyPainShockPercent)).GetFloat();

            XElement itemsNode = root.Element(nameof(Wounds))!;
            Wounds = itemsNode.Elements(nameof(Wound)).Select(x => new Wound(x)).ToDictionary(x => x.Key);

            XElement takedownNode = root.Element("Takedown");
            TakedownRagdollDurationMs = takedownNode.GetInt("RagdollDurationMs");
            TakedownMults = new DBPContainer(takedownNode, isMult: true);
        }

        public void Validate(MainConfig mainConfig, ILogger logger) { }

        public bool IsBleedingCanBeBandaged(float severity) {
            return severity <= GlobalMultipliers.bleed * MAX_SEVERITY_FOR_BANDAGE;
        }

        public static int ConvertHealthFromNative(int health) {
            return health - HEALTH_CORRECTION;
        }

        public static int ConvertHealthToNative(int health) {
            return health + HEALTH_CORRECTION;
        }

        public override string ToString() {
            return $"{nameof(WoundConfig)}:\n"
                   + $"Mults: {GlobalMultipliers.ToString()}\n"
                   + $"Deviations: {GlobalDeviations.ToString()}\n"
                   + $"{nameof(MoveRateOnFullPain)}: {MoveRateOnFullPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(MoveRateOnLegsTrauma)}: {MoveRateOnLegsTrauma.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(RagdollOnPainfulWound)}: {RagdollOnPainfulWound.ToString()}";
        }
    }
}