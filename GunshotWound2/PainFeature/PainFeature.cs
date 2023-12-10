namespace GunshotWound2.PainFeature {
    using System.Windows.Forms;
    using Scellecs.Morpeh;

    public static class PainFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new PainInitSystem(sharedData));
            systemsGroup.AddSystem(new PainChangeSystem(sharedData));

#if DEBUG
            sharedData.inputListener.RegisterHotkey(Keys.Down, () => {
                if (sharedData.TryGetPlayer(out Entity player)) {
                    player.GetComponent<Pain>().diff += sharedData.random.Next(39, 42);
                }
            });
#endif
        }
    }
}