namespace GunshotWound2.StatusFeature {
    using Scellecs.Morpeh;

    public static class StatusFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PedStatusSystem(sharedData));
            systemsGroup.AddSystem(new UnconsciousVisualSystem(sharedData));
        }
    }
}