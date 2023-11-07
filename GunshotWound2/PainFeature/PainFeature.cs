namespace GunshotWound2.PainFeature {
    using Scellecs.Morpeh;

    public static class PainFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PainInitSystem(sharedData));
            systemsGroup.AddSystem(new PainChangeSystem(sharedData));
        }
    }
}