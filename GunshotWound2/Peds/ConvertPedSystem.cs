namespace GunshotWound2.Peds {
    using System;
    using GTA;
    using Scellecs.Morpeh;

    public sealed class ConvertPedSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter justConverted;

        public Scellecs.Morpeh.World World { get; set; }

        public ConvertPedSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            justConverted = World.Filter.With<JustConvertedMarker>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in justConverted) {
                entity.RemoveComponent<JustConvertedMarker>();
            }

            WorldService worldService = sharedData.worldService;
            while (worldService.TryGetToConvert(out Ped pedToConvert)) {
                Scellecs.Morpeh.Entity entity = World.CreateEntity();

                ref ConvertedPed convertedPed = ref entity.AddComponent<ConvertedPed>();
                convertedPed.name = $"P{pedToConvert.Handle.ToString()}";
                convertedPed.thisPed = pedToConvert;
                worldService.AddConverted(pedToConvert, entity);

#if DEBUG
                convertedPed.customBlip = pedToConvert.AddBlip();
                convertedPed.customBlip.Scale = 0.3f;
#endif

                // ecsWorld.ScheduleEventWithTarget<NoPainChangeStateEvent>(entity); TODO
            }
        }
    }
}