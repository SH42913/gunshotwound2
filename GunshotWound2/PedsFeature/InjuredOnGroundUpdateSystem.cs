namespace GunshotWound2.PedsFeature {
    using System;
    using GTA.NaturalMotion;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class InjuredOnGroundUpdateSystem : ILateSystem {
        private readonly SharedData sharedData;

        private Filter peds;
        private Stash<ConvertedPed> pedStash;

        public EcsWorld World { get; set; }

        public InjuredOnGroundUpdateSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            peds = World.Filter.With<ConvertedPed>();
            pedStash = World.GetStash<ConvertedPed>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (EcsEntity entity in peds) {
                ref ConvertedPed convertedPed = ref pedStash.Get(entity);
                if (!convertedPed.lastAggressor.IsValid()) {
                    continue;
                }

                if (convertedPed.activeNmHelper is InjuredOnGroundHelper injuredOnGroundHelper) {
                    injuredOnGroundHelper.AttackerPos = convertedPed.lastAggressor.Position;
                    injuredOnGroundHelper.Update();
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}