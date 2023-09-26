namespace GunshotWound2.Player {
    using Scellecs.Morpeh;

    public static class PlayerFeature {
        public static void CreateSystems(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PlayerDetectSystem(sharedData));
        }
    }
}