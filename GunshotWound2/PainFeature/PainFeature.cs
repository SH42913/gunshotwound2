﻿namespace GunshotWound2.PainFeature {
    using Scellecs.Morpeh;

    public static class PainFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PainInitSystem(sharedData));
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
                    pain.diff += Configs.WoundConfig.DEADLY_PAIN_PERCENT * pain.max;
                }
            });
#endif
        }
    }
}