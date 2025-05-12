namespace GunshotWound2.PedsFeature {
    using GTA;
    using GTA.Math;
    using Scellecs.Morpeh;

    public sealed class RemoveConvertedPedSystem : ICleanupSystem {
        private readonly SharedData sharedData;

        private Filter converted;
        private Stash<ConvertedPed> convertedStash;

        public Scellecs.Morpeh.World World { get; set; }

        public RemoveConvertedPedSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            converted = World.Filter.With<ConvertedPed>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            Vector3 playerPosition = Game.Player.Character.Position;
            float removeRange = sharedData.mainConfig.pedsConfig.RemovePedRange;
            foreach (Scellecs.Morpeh.Entity entity in converted) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (convertedPed.forceRemove || !ped.Exists() || ped.IsDead) {
                    Remove(entity, ref convertedPed);
                } else if (removeRange > 0 && GTA.World.GetDistance(playerPosition, ped.Position) > removeRange) {
                    Remove(entity, ref convertedPed);
                } else {
                    convertedPed.lastFrameHealth = ped.Health;
                    convertedPed.lastFrameArmor = ped.Armor;
                }
            }
        }

        private void Remove(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                sharedData.cameraService.ClearAllEffects();
            }

#if DEBUG
            DeleteBlip(convertedPed);
#endif
            sharedData.worldService.RemoveConverted(convertedPed.thisPed);
            World.RemoveEntity(entity);
        }

        public void Dispose() {
            foreach (Scellecs.Morpeh.Entity entity in converted) {
#if DEBUG
                DeleteBlip(convertedStash.Get(entity));
#endif
            }
        }

#if DEBUG
        private static void DeleteBlip(in ConvertedPed convertedPed) {
            if (convertedPed.customBlip != null) {
                convertedPed.customBlip.Delete();
            }
        }
#endif
    }
}