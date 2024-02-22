namespace GunshotWound2.PainFeature.States {
    using Configs;
    using PedsFeature;
    using Scellecs.Morpeh;

    public interface IPainState {
        float PainThreshold { get; }
        string Color { get; }
        void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed);
        void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed);
        bool TryGetMoveSets(MainConfig mainConfig, in ConvertedPed convertedPed, out string[] moveSets);
    }
}