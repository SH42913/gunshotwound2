// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using TraumaFeature;
    using Utils;

    public sealed class TraumaConfig : MainConfig.IConfig {
        public readonly struct Trauma {
            public readonly string Key;
            public readonly string LocKey;
            public readonly DBPContainer DBP;
            public readonly Traumas.Effects Effect;
            public readonly bool EffectMessage;
            public readonly float PainRateWhenMoving;
            public readonly float PainRateWhenRunning;
            public readonly float PainRateWhenAiming;

            public bool CanGeneratePain => PainRateWhenMoving > 0f || PainRateWhenRunning > 0f || PainRateWhenAiming > 0f;

            public Trauma(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                DBP = new DBPContainer(node, isMult: false);
                Enum.TryParse(node.GetString(nameof(Effect)), out Effect);
                EffectMessage = node.GetBool(nameof(EffectMessage), defaultValue: true);
                PainRateWhenMoving = node.GetFloat(nameof(PainRateWhenMoving));
                PainRateWhenRunning = node.GetFloat(nameof(PainRateWhenRunning));
                PainRateWhenAiming = node.GetFloat(nameof(PainRateWhenAiming));
            }
        }

        public Dictionary<string, Trauma> Traumas;

        public string sectionName => "Traumas.xml";

        public void FillFrom(XDocument doc) {
            Traumas = doc.Element(nameof(Traumas))!
                         .Elements(nameof(Trauma))
                         .Select(x => new Trauma(x))
                         .ToDictionary(x => x.Key);
        }

        public void Validate(MainConfig mainConfig, ILogger logger) { }
    }
}