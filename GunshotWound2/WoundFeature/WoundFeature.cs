namespace GunshotWound2.WoundFeature {
    using Configs;
    using HitDetection;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public static class WoundFeature {
        public static void Create(SystemsGroup systemsGroup, SharedData sharedData) {
            systemsGroup.AddSystem(new WoundSystem(sharedData));
            systemsGroup.AddSystem(new WoundHitDataSystem(sharedData));

#if DEBUG
            sharedData.cheatListener.Register("GSW_RANDOM_HIT", () => {
                if (sharedData.TryGetPlayer(out Entity entity)) {
                    WeaponConfig weaponConfig = sharedData.mainConfig.weaponConfig;
                    BodyPartConfig bodyPartConfig = sharedData.mainConfig.bodyPartConfig;
                    BodyPartConfig.BodyPart bodyPart = sharedData.random.Next(bodyPartConfig.BodyParts);
                    int boneId = sharedData.random.NextFromCollection(bodyPart.Bones);
                    entity.SetComponent(new PedHitData {
                        weaponType = sharedData.random.Next(weaponConfig.Weapons),
                        bodyPart = bodyPart,
                        damagedBone = entity.GetComponent<ConvertedPed>().thisPed.Bones[boneId],
                        hits = 1,
                    });
                }
            });
#endif
        }
    }
}