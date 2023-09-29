namespace GunshotWound2.HealthCare {
    using System;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class HealthInitSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter peds;

        public World World { get; set; }

        public HealthInitSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>().With<JustConvertedMarker>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in peds) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                entity.SetComponent(new Health {
                    max = convertedPed.thisPed.MaxHealth,
                });
            }
        }

        void IDisposable.Dispose() { }
    }
}