namespace GunshotWound2.Peds {
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
            float removeRange = sharedData.mainConfig.NpcConfig.RemovePedRange;
            foreach (Scellecs.Morpeh.Entity entity in converted) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (!ped.Exists() || !ped.IsAlive) {
                    Remove(entity, ref convertedPed);
                } else if (removeRange > 0 && GTA.World.GetDistance(playerPosition, ped.Position) > removeRange) {
                    Remove(entity, ref convertedPed);
                } else {
                    convertedPed.lastFrameHealth = ped.Health;
                }
            }
        }

        private void Remove(Scellecs.Morpeh.Entity entity, ref ConvertedPed convertedPed) {
            sharedData.worldService.RemoveConverted(convertedPed.thisPed);
            World.RemoveEntity(entity);
#if DEBUG
            DeleteBlip(convertedPed);
#endif
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