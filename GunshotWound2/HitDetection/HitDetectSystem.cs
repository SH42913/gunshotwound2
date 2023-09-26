namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using GTA.Native;
    using Peds;
    using Scellecs.Morpeh;

    public sealed class HitDetectSystem : ISystem {
        private readonly SharedData sharedData;

        private Filter converted;
        private Stash<ConvertedPed> convertedStash;

        public Scellecs.Morpeh.World World { get; set; }

        public HitDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            converted = World.Filter.With<ConvertedPed>();
            convertedStash = World.GetStash<ConvertedPed>();
        }

        void IDisposable.Dispose() { }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in converted) {
                ref ConvertedPed convertedPed = ref convertedStash.Get(entity);
                Ped ped = convertedPed.thisPed;
                if (ped.Exists() && ped.IsAlive && CheckDamage(ped)) {
                    entity.AddComponent<PedHitData>();
#if DEBUG
                    sharedData.logger.WriteInfo($"Detect damage at {convertedPed.name}");
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