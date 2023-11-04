namespace GunshotWound2.UI {
    using Scellecs.Morpeh;

    // ReSharper disable once InconsistentNaming
    public static class UIFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.CheckKey, () => HealthChecker.CheckPlayer(sharedData));
        }
    }
}