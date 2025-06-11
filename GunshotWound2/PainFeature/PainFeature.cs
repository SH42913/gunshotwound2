namespace GunshotWound2.PainFeature {
    using InventoryFeature;
    using Scellecs.Morpeh;

    public static class PainFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PainInitSystem(sharedData));
            systemsGroup.AddSystem(new PainGeneratingSystem(sharedData));
            systemsGroup.AddSystem(new PainChangeSystem(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_RANDOM_PAIN", () => {
                if (sharedData.TryGetPlayer(out Entity player)) {
                    player.GetComponent<Pain>().diff += sharedData.random.Next(39, 42);
                }
            });

            sharedData.cheatListener.Register("GSW_DEAD_PAIN", () => {
                if (sharedData.TryGetPlayer(out Entity player)) {
                    ref Pain pain = ref player.GetComponent<Pain>();
                    pain.diff += sharedData.mainConfig.woundConfig.DeadlyPainShockPercent * pain.max;
                }
            });
#endif

            sharedData.cheatListener.Register("GSW_FREE_PAINKILLERS", () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    entity.SetComponent(new AddItemRequest {
                        items = new (string Key, int Count)[] { (PainkillersItem.KEY, 3) },
                    });
                }
            });
        }

        public static void UsePainkillers(Entity target, Entity medic = null) {
            medic ??= target;
            medic.SetComponent(new UseItemRequest {
                target = target,
                item = PainkillersItem.template,
            });
        }
    }
}