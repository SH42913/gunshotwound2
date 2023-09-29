namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using GTA.Native;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class HitDetectSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter convertedPeds;
        private Stash<ConvertedPed> convertedStash;

        public Scellecs.Morpeh.World World { get; set; }

        public HitDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            convertedPeds = World.Filter.With<ConvertedPed>().Without<JustConvertedMarker>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in convertedPeds) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (ped.Exists() && ped.IsAlive && CheckDamage(ped)) {
                    int healthDiff = convertedPed.lastFrameHealth - ped.Health;
                    entity.SetComponent(new PedHitData {
                        healthDiff = healthDiff,
                    });

#if DEBUG
                    sharedData.logger.WriteInfo($"Detect damage at {convertedPed.name}, healthDiff = {healthDiff.ToString()}");
#endif
                }
            }
        }

        private static bool CheckDamage(Ped ped) {
            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, ped); //TODO: maybe ped.HasBeenDamagedByAnyWeapon()?

            // || Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_OBJECT, ped); TODO: We need it?
        }
    }
}