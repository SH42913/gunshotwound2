namespace GunshotWound2.HealthFeature {
    using GTA;
    using InventoryFeature;
    using Scellecs.Morpeh;

    public static class HealthFeature {
        public static void Create(EcsWorld world, SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new TotalHealCheckSystem(sharedData));
            systemsGroup.AddSystem(new HealthInitSystem(sharedData));
            systemsGroup.AddSystem(new SelfHealingSystem(sharedData));
            systemsGroup.AddSystem(new HealthChangeSystem(sharedData));
            systemsGroup.AddSystem(new BleedingSystem(sharedData));
            systemsGroup.AddSystem(new BleedingFxSystem(sharedData));
            systemsGroup.AddSystem(new BloodPoolFxSystem(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HealKey, () => {
                Ped ped = Game.Player.Character;
                ped.Health = ped.MaxHealth;
            });

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.DeathKey, () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    playerEntity.GetComponent<Health>().InstantKill(reason: null);
                }
            });

#if DEBUG
            sharedData.cheatListener.Register("GSW_TEST_BLEED", () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    ref Bleeding bleeding = ref world.CreateEntity().AddComponent<Bleeding>();
                    bleeding.target = playerEntity;
                    bleeding.severity = 0.1f;
                    bleeding.name = "TEST";
                }
            });
#endif

            sharedData.cheatListener.Register("GSW_KILL_PLAYER", () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    playerEntity.GetComponent<Health>().InstantKill("GSW_KILL_PLAYER");
                }
            });

            sharedData.cheatListener.Register("GSW_FREE_BANDAGES", () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    playerEntity.SetComponent(new AddItemRequest {
                        items = [(BandageItem.KEY, 5),],
                    });
                }
            });
        }

        public static void StartBandaging(EcsEntity target, EcsEntity medic = null) {
            medic ??= target;
            medic.SetComponent(new UseItemRequest {
                target = target,
                item = BandageItem.template,
            });
        }

        public static float CalculateSeverityOfAllBleedingWounds(EcsEntity entity) {
            ref Health health = ref entity.GetComponent<Health>(out bool exist);
            if (!exist || !health.HasBleedingWounds()) {
                return 0f;
            }

            var totalSeverity = 0f;
            Stash<Bleeding> bleedingStash = entity.world.GetStash<Bleeding>();
            foreach (EcsEntity woundEntity in health.bleedingWounds) {
                totalSeverity += bleedingStash.Get(woundEntity).severity;
            }

            return totalSeverity;
        }
    }
}