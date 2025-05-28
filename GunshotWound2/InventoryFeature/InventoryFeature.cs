namespace GunshotWound2.InventoryFeature {
    using Scellecs.Morpeh;

    public static class InventoryFeature {
        public delegate bool ItemAction(SharedData sharedData, Entity owner, Entity target, out string message);

        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new InventoryInitSystem(sharedData));
            systemsGroup.AddSystem(new AddItemSystem(sharedData));
            systemsGroup.AddSystem(new InventoryUseSystem(sharedData));

#if DEBUG
#endif
        }

        public static ItemTemplate GetTemplateByKey(string key) {
            switch (key) {
                case HealthFeature.BandageItem.KEY: return HealthFeature.BandageItem.template;
                default:                            return default;
            }
        }
    }
}