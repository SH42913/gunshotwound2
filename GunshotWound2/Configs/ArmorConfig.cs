// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Xml.Linq;
    using Utils;

    public sealed class ArmorConfig {
        public float MinimalChanceForArmorSave;

        public void FillFrom(XElement doc) {
            XElement node = doc.Element("Armor");
            if (node == null) {
                return;
            }

            MinimalChanceForArmorSave = node.Element("MinimalChanceForArmorSave").GetFloat();
        }
    }
}