// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Linq;
    using System.Xml.Linq;
    using Utils;

    public sealed class InventoryConfig : MainConfig.IConfig {
        public readonly struct Loadout {
            public readonly (string Key, int Count)[] items;

            public Loadout((string, int)[] items) {
                this.items = items;
            }
        }

        public bool BlipsToMedkits;
        public float TimeToRefreshMedkits;
        public string MedkitModel;

        public float PainkillersRate;
        public float PainkillersDuration;

        public Loadout DefaultLoadout;
        public Loadout MedkitLoadout;
        public Loadout EmergencyVehicleLoadout;

        public string sectionName => "Inventory.xml";

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(InventoryConfig))!;
            DefaultLoadout = GetLoadout(root, nameof(DefaultLoadout));
            MedkitLoadout = GetLoadout(root, nameof(MedkitLoadout));
            EmergencyVehicleLoadout = GetLoadout(root, nameof(EmergencyVehicleLoadout));

            XElement medkitNode = root.Element(nameof(BlipsToMedkits));
            BlipsToMedkits = medkitNode.GetBool();
            TimeToRefreshMedkits = medkitNode.GetFloat("RefreshTime");
            MedkitModel = medkitNode.GetString("ModelName");

            XElement painkillersNode = root.Element("Painkillers");
            PainkillersRate = painkillersNode.GetFloat("Rate");
            PainkillersDuration = painkillersNode.GetFloat("Duration");
        }

        public void Validate(MainConfig mainConfig, ILogger logger) { }

        private static Loadout GetLoadout(XElement root, string loadoutName) {
            XElement node = root.Element(loadoutName)!;

            (string key, int count)[] items = node.Elements("Item")
                                                  .Select(x => {
                                                      string key = x.GetString("Key");
                                                      int count = x.GetInt("Count");
                                                      return (key, count);
                                                  })
                                                  .ToArray();

            return new Loadout(items);
        }
    }
}