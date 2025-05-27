namespace GunshotWound2.HitDetection {
    using System;
    using Configs;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class BodyHitSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        private BodyPartConfig BodyPartConfig => sharedData.mainConfig.bodyPartConfig;

        public BodyHitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity pedEntity in damagedPeds) {
                ref PedHitData hitData = ref pedEntity.GetComponent<PedHitData>();
                if (ShouldSkipDetection(ref hitData)) {
                    continue;
                }

                ref ConvertedPed convertedPed = ref pedEntity.GetComponent<ConvertedPed>();
                if (hitData.useRandomBodyPart) {
                    hitData.bodyPart = sharedData.random.Next(BodyPartConfig.BodyParts);
#if DEBUG
                    sharedData.logger.WriteInfo($"Damaged random part is {hitData.bodyPart.Key} of {convertedPed.name}");
#endif
                } else {
                    Bone damagedBone = convertedPed.thisPed.Bones.LastDamaged.Tag;
                    convertedPed.lastDamagedBone = damagedBone;

                    hitData.bodyPart = BodyPartConfig.GetBodyPartByBone(damagedBone);
#if DEBUG
                    sharedData.logger.WriteInfo($"Damaged part is {hitData.bodyPart.Key}, bone {damagedBone} at {convertedPed.name}");
#endif
                }
            }
        }

        private bool ShouldSkipDetection(ref PedHitData hitData) {
            if (!hitData.weaponType.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip body part detection, 'cause there's no weapon");
#endif
                return true;
            }

            if (hitData.bodyPart.IsValid) {
#if DEBUG
                sharedData.logger.WriteInfo("Skip body part detection, 'cause it's already detected");
#endif
                return true;
            }

            return false;
        }
    }
}