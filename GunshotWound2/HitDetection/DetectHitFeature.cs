namespace GunshotWound2.HitDetection {
    using Scellecs.Morpeh;

    public static class DetectHitFeature {
        public static void CreateSystems(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new HitDetectSystem(sharedData));
            systemsGroup.AddSystem(new WeaponHitSystem(sharedData));
            systemsGroup.AddSystem(new BodyHitSystem(sharedData));
            systemsGroup.AddSystem(new HitCleanSystem());
        }
    }
}