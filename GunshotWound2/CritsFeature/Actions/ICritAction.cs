namespace GunshotWound2.CritsFeature.Actions {
    using PedsFeature;
    using Scellecs.Morpeh;

    public interface ICritAction {
        void Apply(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed);
        void Cancel(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed);
    }
}