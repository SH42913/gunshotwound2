namespace GunshotWound2.CritsFeature {
    using Scellecs.Morpeh;

    public static class CritsFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new CritsSystem(sharedData));

#if DEBUG
            sharedData.inputListener.RegisterHotkey(System.Windows.Forms.Keys.V, () => {
                if (sharedData.TryGetPlayer(out Entity player)) {
                    System.Type enumType = typeof(HitDetection.PedHitData.BodyParts);
                    var bodyParts = (HitDetection.PedHitData.BodyParts[])System.Enum.GetValues(enumType);
                    int randomIndex = sharedData.random.Next(0, bodyParts.Length);
                    player.AddOrGetComponent<Crits>().requestBodyPart = bodyParts[randomIndex];
                }
            });
#endif
        }
    }
}