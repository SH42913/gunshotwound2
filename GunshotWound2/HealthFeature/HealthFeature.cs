namespace GunshotWound2.HealthFeature {
    using InventoryFeature;
    using Scellecs.Morpeh;

    public static class HealthFeature {
        public static void Create(World world, SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new TotalHealCheckSystem(sharedData));
            systemsGroup.AddSystem(new HealthInitSystem(sharedData));
            systemsGroup.AddSystem(new BleedingSystem(sharedData));
            systemsGroup.AddSystem(new SelfHealingSystem(sharedData));
            systemsGroup.AddSystem(new HealthChangeSystem(sharedData));

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.HealKey, () => {
                GTA.Ped ped = GTA.Game.Player.Character;
                ped.Health = ped.MaxHealth;
            });

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.DeathKey, () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    entity.GetComponent<Health>().InstantKill(reason: null);
                }
            });

#if DEBUG
            sharedData.cheatListener.Register("GSW_TEST_BLEED", () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    ref Bleeding bleeding = ref world.CreateEntity().AddComponent<Bleeding>();
                    bleeding.target = entity;
                    bleeding.severity = 0.1f;
                    bleeding.name = "TEST";
                }
            });
#endif

            sharedData.cheatListener.Register("GSW_KILL_PLAYER", () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    entity.GetComponent<Health>().InstantKill("GSW_KILL_PLAYER");
                }
            });

            sharedData.cheatListener.Register("GSW_FREE_BANDAGES", () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    entity.SetComponent(new AddItemRequest {
                        item = (BandageItem.template, count: 5),
                    });
                }
            });
        }

        public static void StartBandaging(Entity target, Entity medic = null) {
            medic ??= target;
            medic.SetComponent(new UseItemRequest {
                target = target,
                item = BandageItem.template,
            });
        }

        public static float CalculateSeverityOfAllBleedingWounds(Entity entity) {
            ref Health health = ref entity.GetComponent<Health>();
            if (!health.HasBleedingWounds()) {
                return 0f;
            }

            var totalSeverity = 0f;
            Stash<Bleeding> bleedingStash = entity.world.GetStash<Bleeding>();
            foreach (Entity woundEntity in health.bleedingWounds) {
                totalSeverity += bleedingStash.Get(woundEntity).severity;
            }

            return totalSeverity;
        }
    }
}