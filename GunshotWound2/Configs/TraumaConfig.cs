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
            public readonly Traumas.Effects Effect;
            public readonly bool EffectMessage;
            public readonly float Damage;
            public readonly float Bleed;
            public readonly float Pain;

            public Trauma(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                Enum.TryParse(node.GetString(nameof(Effect)), out Effect);
                EffectMessage = node.GetBool(nameof(EffectMessage), defaultValue: true);
                Damage = node.GetFloat(nameof(Damage));
                Bleed = node.GetFloat(nameof(Bleed));
                Pain = node.GetFloat(nameof(Pain));
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