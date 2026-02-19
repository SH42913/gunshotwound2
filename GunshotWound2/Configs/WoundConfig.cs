// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using GTA;
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

        public readonly struct BloodPoolData {
            public readonly ParticleEffectAsset asset;
            public readonly string effectName;
            public readonly float growTime;

            public BloodPoolData(XElement element) {
                asset = new ParticleEffectAsset(element.GetString("Asset"));
                effectName = element.GetString("Effect");
                growTime = element.GetFloat("GrowTime");
            }
        }

        public readonly struct BleedingFxData {
            public readonly ParticleEffectAsset asset;
            public readonly string effectName;
            public readonly float severity;

            public BleedingFxData(XElement element) {
                asset = new ParticleEffectAsset(element.GetString("Asset"));
                effectName = element.GetString("Effect");
                severity = element.GetFloat("Severity");
            }
        }

        private const int HEALTH_CORRECTION = 100;

        public DBPContainer GlobalMultipliers;
        public DBPContainer GlobalDeviations;

        public bool RagdollOnPainfulWound;
        public float PainfulWoundPercent;
        public bool UseCustomUnconsciousBehaviour;
        public float DelayedPainPercent;
        public float DelayedPainSpeed;

        public int TakedownRagdollDurationMs;
        public DBPContainer TakedownMults;

        public Dictionary<string, Wound> Wounds;

        public BleedingFxData[] PenetratingBleedingEffects;
        public BleedingFxData[] BluntBleedingEffects;
        public BloodPoolData[] BloodPoolParticles;
        public float BloodPoolGrowTimeScale;

        public string sectionName => "Wounds.xml";

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(WoundConfig))!;

            GlobalMultipliers = new DBPContainer(root.Element(nameof(GlobalMultipliers)), isMult: true);
            GlobalDeviations = new DBPContainer(root.Element(nameof(GlobalDeviations)), isMult: false);

            RagdollOnPainfulWound = root.Element(nameof(RagdollOnPainfulWound)).GetBool();
            PainfulWoundPercent = root.Element(nameof(PainfulWoundPercent)).GetFloat();
            UseCustomUnconsciousBehaviour = root.Element(nameof(UseCustomUnconsciousBehaviour)).GetBool();
            DelayedPainPercent = root.Element(nameof(DelayedPainPercent)).GetFloat();
            DelayedPainSpeed = root.Element(nameof(DelayedPainSpeed)).GetFloat();

            XElement itemsNode = root.Element(nameof(Wounds))!;
            Wounds = itemsNode.Elements(nameof(Wound)).Select(x => new Wound(x)).ToDictionary(x => x.Key);

            XElement takedownNode = root.Element("Takedown");
            TakedownRagdollDurationMs = takedownNode.GetInt("RagdollDurationMs");
            TakedownMults = new DBPContainer(takedownNode, isMult: true);

            const string particle = "Particle";
            XElement bleedFxNode = root.Element("BleedingEffects")!;
            PenetratingBleedingEffects = bleedFxNode.Element("Penetrating")!.Elements(particle)
                                                    .Select(x => new BleedingFxData(x))
                                                    .OrderByDescending(x => x.severity)
                                                    .ToArray();

            BluntBleedingEffects = bleedFxNode.Element("Blunt")!.Elements(particle)
                                              .Select(x => new BleedingFxData(x))
                                              .OrderByDescending(x => x.severity)
                                              .ToArray();

            XElement bloodPoolsNode = root.Element("BloodPools")!;
            BloodPoolGrowTimeScale = bloodPoolsNode.GetFloat("GrowTimeScale", 1f);
            BloodPoolParticles = bloodPoolsNode.Elements(particle).Select(x => new BloodPoolData(x)).ToArray();
        }

        public void Validate(MainConfig mainConfig, ILogger logger) { }

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
                   + $"{nameof(RagdollOnPainfulWound)}: {RagdollOnPainfulWound.ToString()}";
        }
    }
}