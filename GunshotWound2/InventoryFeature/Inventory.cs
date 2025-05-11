namespace GunshotWound2.InventoryFeature {
    using System;
    using System.Collections.Generic;
    using Scellecs.Morpeh;

    [Serializable]
    public struct Inventory : IComponent {
        public List<(ItemTemplate template, int count)> items;
        public int modelHash;
    }

    public static class InventoryExtensions {
        public static int AmountOf(this in Inventory inventory, ItemTemplate itemTemplate) {
            if (inventory.items == null) {
                return 0;
            }

            foreach ((ItemTemplate template, int count) in inventory.items) {
                if (itemTemplate.Equals(template)) {
                    return count;
                }
            }

            return 0;
        }

        public static bool Has(this in Inventory inventory, ItemTemplate itemTemplate) {
            return inventory.AmountOf(itemTemplate) > 0;
        }

        public static bool Consume(this ref Inventory inventory, ItemTemplate itemTemplate, int amount = 1) {
            for (var i = 0; i < inventory.items.Count; i++) {
                (ItemTemplate template, int count) = inventory.items[i];
                if (!itemTemplate.Equals(template)) {
                    continue;
                }

                if (count >= amount) {
                    inventory.items[i] = (template, count - 1);
                    return true;
                } else {
                    break;
                }
            }

            return false;
        }
    }
}