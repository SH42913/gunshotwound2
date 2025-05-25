namespace GunshotWound2.WoundFeature {
    using HitDetection;
    using Scellecs.Morpeh;

    public static class WoundFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new WoundSystem(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_RANDOM_HIT", () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    entity.SetComponent(new PedHitData {
                        weaponType = sharedData.mainConfig.weaponConfig.AllWeapons[0],
                        bodyPart = sharedData.mainConfig.bodyPartConfig.GetBodyPartByKey("Chest"),
                    });
                }
            });
#endif
        }
    }
}