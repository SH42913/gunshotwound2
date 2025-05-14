namespace GunshotWound2.InventoryFeature {
    using System;
    using System.Collections.Generic;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class InventoryInitSystem : ISystem {
        private readonly SharedData sharedData;
        private Stash<ConvertedPed> convertedStash;
        private Stash<Inventory> inventoryStash;
        private Filter justConvertedPeds;

        public World World { get; set; }

        public InventoryInitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            convertedStash = World.GetStash<ConvertedPed>();
            inventoryStash = World.GetStash<Inventory>();
            justConvertedPeds = World.Filter.With<ConvertedPed>().With<JustConvertedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in justConvertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                if (!convertedPed.isPlayer) {
                    continue;
                }

                if (inventoryStash.Has(entity)) {
                    continue;
                }

                ref Inventory inventory = ref inventoryStash.Add(entity);
                inventory.items = new List<(ItemTemplate template, int count)>(capacity: 4);
                inventory.modelHash = convertedPed.thisPed.Model.Hash;
#if DEBUG
                sharedData.logger.WriteInfo($"Created inventory {inventory.modelHash} for ped {convertedPed.name}");
#endif

                //TODO: Load state
                sharedData.mainConfig.inventoryConfig.DefaultLoadout.ApplyToInventory(ref inventory);
            }
        }

        void IDisposable.Dispose() { }
    }
}