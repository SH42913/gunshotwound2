namespace GunshotWound2.InventoryFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class AddItemSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter requests;

        public World World { get; set; }

        public AddItemSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            requests = World.Filter.With<Inventory>().With<AddItemRequest>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in requests) {
                var request = entity.GetComponent<AddItemRequest>();
                entity.RemoveComponent<AddItemRequest>();

                bool isPlayer = entity.GetComponent<ConvertedPed>().isPlayer;
                ref Inventory inventory = ref entity.GetComponent<Inventory>();
                foreach ((string key, int count) in request.items) {
                    ItemTemplate item = InventoryFeature.GetTemplateByKey(key);
                    if (!item.IsValid) {
                        sharedData.logger.WriteError($"There's no item template with key {key}");
                        continue;
                    }

                    if (TryAddItem(ref inventory, item, count) && isPlayer) {
                        string itemCountString = item.GetPluralTranslation(sharedData.localeConfig, count);
                        string notification = $"{sharedData.localeConfig.FoundItems} {itemCountString}";
                        sharedData.notifier.info.QueueMessage(notification, Notifier.Color.GREEN);
                    }
                }
            }
        }

        void IDisposable.Dispose() { }

        private bool TryAddItem(ref Inventory inventory, ItemTemplate item, int count) {
            if (!item.IsValid || count < 1) {
                return false;
            }

            AddItemToInventory(ref inventory, item, count);
            int totalAmount = inventory.AmountOf(item);
#if DEBUG
            sharedData.logger.WriteInfo($"Added {count} of {item.key} to "
                                        + $"Inventory of {inventory.modelHash}, "
                                        + $"total count = {totalAmount}");
#endif
            return true;
        }

        private static void AddItemToInventory(ref Inventory inventory, ItemTemplate newItemTemplate, int count) {
            for (var i = 0; i < inventory.items.Count; i++) {
                (ItemTemplate template, int inventoryCount) = inventory.items[i];
                if (newItemTemplate.Equals(template)) {
                    inventory.items[i] = (template, inventoryCount + count);
                    return;
                }
            }

            inventory.items.Add((newItemTemplate, count));
        }
    }
}