// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using CritsFeature;
    using Utils;

    public sealed class CritsConfig {
        public readonly struct Crit {
            public readonly string Key;
            public readonly string LocKey;
            public readonly Crits.Effects Effect;
            public readonly bool Message;
            public readonly float Damage;
            public readonly float Bleed;
            public readonly float Pain;

            public Crit(XElement node) {
                Key = node.GetString(nameof(Key));
                LocKey = node.GetString(nameof(LocKey));
                Enum.TryParse(node.GetString(nameof(Effect)), out Effect);
                Message = node.GetBool(nameof(Message));
                Damage = node.GetFloat(nameof(Damage));
                Bleed = node.GetFloat(nameof(Bleed));
                Pain = node.GetFloat(nameof(Pain));
            }
        }

        public Crit[] Crits;

        public void FillFrom(XDocument doc) {
            XElement critsNode = doc.Element(nameof(Crits))!;
            Crits = critsNode.Elements(nameof(Crit)).Select(x => new Crit(x)).ToArray();
        }

        public Crit GetCritByKey(string key) {
            foreach (Crit crit in Crits) {
                if (crit.Key == key) {
                    return crit;
                }
            }

            throw new Exception($"There's no Crit with key {key}");
        }
    }
}