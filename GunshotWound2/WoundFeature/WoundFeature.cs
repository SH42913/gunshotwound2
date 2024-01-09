namespace GunshotWound2.WoundFeature {
    using System.Windows.Forms;
    using HitDetection;
    using Scellecs.Morpeh;

    public static class WoundFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new WoundSystem(sharedData));

#if DEBUG
            sharedData.inputListener.RegisterHotkey(Keys.Right, () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    entity.SetComponent(new PedHitData {
                        weaponType = PedHitData.WeaponTypes.SmallCaliber,
                        bodyPart = PedHitData.BodyParts.UpperBody,
                    });
                }
            });
#endif
        }
    }
}