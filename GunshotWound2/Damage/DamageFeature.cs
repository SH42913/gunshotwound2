namespace GunshotWound2.Damage {
    using Scellecs.Morpeh;

    public static class DamageFeature {
        public static void CreateSystems(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new WoundSystem(sharedData));
        }
    }
}