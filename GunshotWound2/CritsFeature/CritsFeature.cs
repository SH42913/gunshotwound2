namespace GunshotWound2.CritsFeature {
    using Configs;
    using Scellecs.Morpeh;
    using Utils;

    public static class CritsFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new CritsSystem(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_RANDOM_CRIT", () => {
                if (sharedData.TryGetPlayer(out Entity player)) {
                    BodyPartConfig.BodyPart[] bodyParts = sharedData.mainConfig.bodyPartConfig.BodyParts;
                    player.AddOrGetComponent<Crits>().requestBodyPart = sharedData.random.Next(bodyParts);
                }
            });
#endif
        }
    }
}