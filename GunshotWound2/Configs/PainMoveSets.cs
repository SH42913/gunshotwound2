namespace GunshotWound2.Configs {
    using System;
    using System.Xml.Linq;

    public readonly struct PainMoveSets {
        public readonly string[] mild;
        public readonly string[] average;
        public readonly string[] intense;

        public PainMoveSets(string[] mild, string[] average, string[] intense) {
            this.mild = mild;
            this.average = average;
            this.intense = intense;
        }

        public static PainMoveSets FromXElement(XElement node, string nodeName) {
            XElement setsNode = node.Element(nodeName);
            if (setsNode == null) {
                return default;
            }

            return new PainMoveSets(mild: GetFromAttribute("MildPain"),
                                    average: GetFromAttribute("AvgPain"),
                                    intense: GetFromAttribute("IntensePain"));

            string[] GetFromAttribute(string name) {
                return setsNode.Attribute(name)?.Value.Split(MainConfig.Separator, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}