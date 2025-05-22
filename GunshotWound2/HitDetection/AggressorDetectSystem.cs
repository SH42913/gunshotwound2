namespace GunshotWound2.HitDetection {
    using System;
    using GTA;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class AggressorDetectSystem : ISystem {
        private readonly SharedData sharedData;
        private Filter damagedPeds;

        public Scellecs.Morpeh.World World { get; set; }

        public AggressorDetectSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            damagedPeds = World.Filter.With<ConvertedPed>().With<PedHitData>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Scellecs.Morpeh.Entity entity in damagedPeds) {
                ref PedHitData hitData = ref entity.GetComponent<PedHitData>();
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                if (!PedEffects.TryGetLastDamageRecord(convertedPed.thisPed, out uint weaponHash, out int handle, out int time)) {
                    continue;
                }

                int diffTime = Game.GameTime - time;
#if DEBUG
                sharedData.logger.WriteInfo($"Damage record {convertedPed.name} A:{handle} W:{weaponHash}, time={time}({diffTime})");
#endif
                if (diffTime <= 20 && PedEffects.TryGetPedByHandle(handle, out Ped aggressor) && hitData.weaponHash == weaponHash) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Confirmed aggressor for {convertedPed.name} is {aggressor.Handle}");
#endif
                    convertedPed.lastAggressor = aggressor;
                } else {
                    convertedPed.lastAggressor = null;
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}