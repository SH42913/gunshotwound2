namespace GunshotWound2.Player {
    using Scellecs.Morpeh;

    public static class PlayerFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PlayerDetectSystem(sharedData));
            
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.CheckKey, () => CheckPlayer(sharedData));
        }

        private static void CheckPlayer(SharedData sharedData) {
            if (sharedData.TryGetPlayer(out Entity playerEntity)) {
                HealthChecker.Check(sharedData, playerEntity);
            }
        }
    }
}