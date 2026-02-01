// #define DEBUG_EVERY_FRAME

namespace GunshotWound2.PedsFeature {
    using GTA;
    using Scellecs.Morpeh;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class RemoveConvertedPedSystem : ICleanupSystem {
        private readonly SharedData sharedData;

        private Filter converted;
        private Stash<ConvertedPed> convertedStash;

        public EcsWorld World { get; set; }

        public RemoveConvertedPedSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            converted = World.Filter.With<ConvertedPed>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in converted) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (convertedPed.forceRemove || !ped.Exists() || ped.IsDead) {
                    Remove(entity, ref convertedPed);
                } else {
                    convertedPed.lastFrameHealth = ped.Health;
                    convertedPed.lastFrameArmor = ped.Armor;
                }
            }
        }

        private void Remove(EcsEntity entity, ref ConvertedPed convertedPed) {
#if DEBUG && DEBUG_EVERY_FRAME
            sharedData.logger.WriteInfo($"Removing {convertedPed.name} from GSW world");
#endif
            convertedPed.thisPed.MaxHealth = convertedPed.defaultMaxHealth;
            PedEffects.CleanFacialIdleAnim(convertedPed.thisPed);

            if (convertedPed.isPlayer) {
                sharedData.cameraService.ClearAllEffects();
            }

#if PED_BLIPS
            DeleteBlip(convertedPed);
#endif
            sharedData.worldService.RemoveConverted(convertedPed.thisPed);
            World.RemoveEntity(entity);
        }

        public void Dispose() {
            foreach (EcsEntity entity in converted) {
                Remove(entity, ref convertedStash.Get(entity));
            }
        }

#if PED_BLIPS
        private static void DeleteBlip(in ConvertedPed convertedPed) {
            if (convertedPed.customBlip != null) {
                convertedPed.customBlip.Delete();
            }
        }
#endif
    }
}