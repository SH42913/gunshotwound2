namespace GunshotWound2.HitDetection {
    using System;
    using Scellecs.Morpeh;

    public sealed class HitCleanSystem : ICleanupSystem {
        private Filter hits;
        private Stash<PedHitData> hitsStash;

        public World World { get; set; }

        public void OnAwake() {
            hits = World.Filter.With<PedHitData>();
            hitsStash = World.GetStash<PedHitData>();
        }

        public void OnUpdate(float deltaTime) {
            foreach (Entity entity in hits) {
                hitsStash.Remove(entity);
            }
        }

        void IDisposable.Dispose() { }
    }
}