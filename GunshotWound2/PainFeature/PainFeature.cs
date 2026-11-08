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
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    playerEntity.GetComponent<Pain>().diff += sharedData.random.Next(39, 42);
                }
            });
#endif

            sharedData.cheatListener.Register("GSW_FREE_PAINKILLERS", () => {
                if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                    playerEntity.world.GetRequest<AddItemRequest>().Publish(new AddItemRequest {
                        target = playerEntity,
                        items = [(PainkillersItem.KEY, 3),],
                    });
                }
            });
        }

        public static void UsePainkillers(EcsEntity target, EcsEntity medic = null) {
            medic ??= target;
            medic.world.GetRequest<UseItemRequest>().Publish(new UseItemRequest {
                owner = medic,
                target = target,
                item = PainkillersItem.template,
            });
        }
    }
}