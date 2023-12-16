namespace GunshotWound2.WoundFeature {
    using Scellecs.Morpeh;

    public static class WoundFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new WoundSystem(sharedData));
        }
    }
}