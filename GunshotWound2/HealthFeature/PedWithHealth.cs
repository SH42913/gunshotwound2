namespace GunshotWound2.HealthFeature {
    using PedsFeature;
    using Scellecs.Morpeh;

    public struct PedWithHealth : IAspect, IFilterExtension {
        public EcsEntity Entity { get; set; }

        public readonly ref ConvertedPed Ped => ref pedStash.Get(Entity);
        public readonly ref Health Health => ref healthStash.Get(Entity);

        private Stash<ConvertedPed> pedStash;
        private Stash<Health> healthStash;

        public void OnGetAspectFactory(EcsWorld world) {
            pedStash = world.GetStash<ConvertedPed>();
            healthStash = world.GetStash<Health>();
        }

        public FilterBuilder Extend(FilterBuilder rootFilter) {
            return rootFilter.With<ConvertedPed>().With<Health>();
        }
    }
}