namespace GunshotWound2.HealthCare {
    using Scellecs.Morpeh;

    public static class HealthFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new TotalHealCheckSystem(sharedData));
            systemsGroup.AddSystem(new HealthInitSystem(sharedData));
            systemsGroup.AddSystem(new BandageSystem(sharedData));
            systemsGroup.AddSystem(new BleedingSystem(sharedData));
            systemsGroup.AddSystem(new SelfHealingSystem(sharedData));
            systemsGroup.AddSystem(new HealthChangeSystem(sharedData));
            
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HealKey, () => {
                GTA.Ped ped = GTA.Game.Player.Character;
                ped.Health = ped.MaxHealth;
            });
        }
    }
}