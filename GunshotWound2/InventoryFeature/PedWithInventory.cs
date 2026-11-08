namespace GunshotWound2.InventoryFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public struct PedWithInventory : IAspect, IFilterExtension {
        public EcsEntity Entity { get; set; }

        public readonly ref ConvertedPed Ped => ref pedStash.Get(Entity);
        public readonly ref Inventory Inventory => ref inventoryStash.Get(Entity);

        private Stash<ConvertedPed> pedStash;
        private Stash<Inventory> inventoryStash;

        public void OnGetAspectFactory(EcsWorld world) {
            pedStash = world.GetStash<ConvertedPed>();
            inventoryStash = world.GetStash<Inventory>();
        }

        public FilterBuilder Extend(FilterBuilder rootFilter) {
            return rootFilter.With<ConvertedPed>().With<Inventory>();
        }
    }
}