namespace GunshotWound2.TraumaFeature {
    using Configs;
    using Scellecs.Morpeh;
    using Utils;

    public static class TraumaFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new TraumaSystem(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_RANDOM_CRIT", () => {
                if (sharedData.TryGetPlayer(out Entity player)) {
                    BodyPartConfig.BodyPart[] bodyParts = sharedData.mainConfig.bodyPartConfig.BodyParts;
                    player.world.CreateEntity().SetComponent(new TraumaRequest {
                        target = player,
                        targetBodyPart = sharedData.random.Next(bodyParts),
                    });
                }
            });
#endif
        }
    }
}