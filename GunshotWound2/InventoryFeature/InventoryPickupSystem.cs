namespace GunshotWound2.InventoryFeature {
    using System;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class InventoryPickupSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter requests;

        public World World { get; set; }

        public InventoryPickupSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            requests = World.Filter.With<Inventory>().With<AddItemRequest>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in requests) {
                ref Inventory inventory = ref entity.GetComponent<Inventory>();
                ref AddItemRequest request = ref entity.GetComponent<AddItemRequest>();
                TryAddItem(ref inventory, request.item);
                entity.RemoveComponent<AddItemRequest>();
            }
        }

        void IDisposable.Dispose() { }

        private void TryAddItem(ref Inventory inventory, (ItemTemplate template, int count) tuple) {
            if (!tuple.template.IsValid || tuple.count < 1) {
                return;
            }

            AddItemToInventory(ref inventory, tuple.template, tuple.count);
            int totalAmount = inventory.AmountOf(tuple.template);
#if DEBUG
            sharedData.logger.WriteInfo($"Added {tuple.count} of {tuple.template.internalName} to "
                                        + $"Inventory of {inventory.modelHash}, "
                                        + $"total count = {totalAmount}");
#endif
            var message = $"In your inventory: {totalAmount} of {tuple.template.internalName}"; // TODO: Localize
            sharedData.notifier.ShowOne(message, blinking: false, Notifier.Color.GREEN);
        }

        private static void AddItemToInventory(ref Inventory inventory, ItemTemplate newItemTemplate, int count) {
            if (count < 1) {
                return;
            }

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