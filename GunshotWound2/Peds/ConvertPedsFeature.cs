namespace GunshotWound2.Peds {
    using Scellecs.Morpeh;

    public static class ConvertPedsFeature {
        public static void CreateSystems(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new NpcDetectSystem(sharedData));
            systemsGroup.AddSystem(new ConvertPedSystem(sharedData));
            systemsGroup.AddSystem(new RemoveConvertedPedSystem(sharedData));
        }
    }
}