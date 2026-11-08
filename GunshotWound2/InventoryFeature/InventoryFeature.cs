namespace GunshotWound2.InventoryFeature {
    using HealthFeature;
    using PainFeature;
    using Scellecs.Morpeh;

    public static class InventoryFeature {
        public delegate bool ItemAction(SharedData sharedData, EcsEntity owner, EcsEntity target, out string message);

        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new InventoryInitSystem(sharedData));
            systemsGroup.AddSystem(new AddItemSystem(sharedData));
            systemsGroup.AddSystem(new InventoryUseSystem(sharedData));

#if DEBUG
#endif
        }

        public static ItemTemplate GetTemplateByKey(string key) {
            switch (key) {
                case BandageItem.KEY:     return BandageItem.template;
                case PainkillersItem.KEY: return PainkillersItem.template;
                default:                  return default;
            }
        }
    }
}