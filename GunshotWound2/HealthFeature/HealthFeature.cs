namespace GunshotWound2.HealthFeature {
    using Scellecs.Morpeh;

    public static class HealthFeature {
        public static void Create(World world, SystemsGroup systemsGroup, SharedData sharedData) {
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

#if DEBUG
            sharedData.inputListener.RegisterHotkey(System.Windows.Forms.Keys.B, () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    ref Bleeding bleeding = ref world.CreateEntity().AddComponent<Bleeding>();
                    bleeding.target = entity;
                    bleeding.severity = 0.1f;
                    bleeding.name = "TEST";
                };
            });
#endif
        }
    }
}