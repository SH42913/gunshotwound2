// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using HealthFeature;
    using InventoryFeature;
    using Utils;

    public sealed class InventoryConfig {
        public readonly struct Loadout {
            public readonly List<(ItemTemplate, int)> items;

            public Loadout(List<(ItemTemplate, int)> items) {
                this.items = items;
            }

            public void ApplyToInventory(ref Inventory inventory) {
                foreach ((ItemTemplate, int) item in items) {
                    inventory.items.Add(item);
                }
            }
        }

        private readonly ItemTemplate[] templates = new[] { BandageItem.template, };

        public Loadout DefaultLoadout;

        public void FillFrom(XElement doc) {
            XElement node = doc.Element("Inventory");
            if (node == null) {
                return;
            }

            DefaultLoadout = GetLoadout(node.Element("DefaultLoadout"));
        }

        private Loadout GetLoadout(XElement node) {
            var list = new List<(ItemTemplate, int)>();

            foreach (XElement element in node.Elements("Item")) {
                string name = element.GetString("Name");
                int count = element.GetInt("Count");
                ItemTemplate template = GetItemTemplateByName(name);
                list.Add((template, count));
            }

            return new Loadout(list);
        }

        private ItemTemplate GetItemTemplateByName(string name) {
            foreach (ItemTemplate template in templates) {
                if (template.internalName == name) {
                    return template;
                }
            }

            throw new Exception($"There's no item {name}");
        }
    }
}