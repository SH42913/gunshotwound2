// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Xml.Linq;

    public readonly struct StatusMoveSets {
        public readonly string[] Warning;
        public readonly string[] Distressed;
        public readonly string[] Critical;

        private StatusMoveSets(string[] warning, string[] distressed, string[] critical) {
            Warning = warning;
            Distressed = distressed;
            Critical = critical;
        }

        public static StatusMoveSets FromXElement(XElement node, string nodeName) {
            XElement setsNode = node.Element(nodeName)!;
            return new StatusMoveSets(warning: GetFromAttribute(nameof(Warning)),
                                    distressed: GetFromAttribute(nameof(Distressed)),
                                    critical: GetFromAttribute(nameof(Critical)));

            string[] GetFromAttribute(string name) {
                string value = setsNode.Attribute(name)?.Value;
                return !string.IsNullOrEmpty(value)
                        ? value.Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries)
                        : null;
            }
        }
    }
}