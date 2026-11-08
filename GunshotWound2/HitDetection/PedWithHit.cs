namespace GunshotWound2.HitDetection {
    using PedsFeature;
    using Scellecs.Morpeh;

    public struct PedWithHit : IAspect, IFilterExtension {
        public EcsEntity Entity { get; set; }

        public readonly ref ConvertedPed Ped => ref pedStash.Get(Entity);
        public readonly ref PedHitData Hit => ref hitStash.Get(Entity);

        private Stash<ConvertedPed> pedStash;
        private Stash<PedHitData> hitStash;

        public void OnGetAspectFactory(EcsWorld world) {
            pedStash = world.GetStash<ConvertedPed>();
            hitStash = world.GetStash<PedHitData>();
        }

        public FilterBuilder Extend(FilterBuilder rootFilter) {
            return rootFilter.With<ConvertedPed>().With<PedHitData>();
        }
    }
}