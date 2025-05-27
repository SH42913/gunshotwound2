// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using TraumaFeature;
    using Utils;

    public sealed class TraumaConfig {
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

        public Trauma[] Traumas;

        public void FillFrom(XDocument doc) {
            XElement critsNode = doc.Element(nameof(Traumas))!;
            Traumas = critsNode.Elements(nameof(Trauma)).Select(x => new Trauma(x)).ToArray();
        }

        public Trauma GetTraumaByKey(string key) {
            foreach (Trauma crit in Traumas) {
                if (crit.Key == key) {
                    return crit;
                }
            }

            throw new Exception($"There's no Crit with key {key}");
        }
    }
}