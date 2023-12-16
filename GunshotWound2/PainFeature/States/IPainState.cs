namespace GunshotWound2.PainFeature.States {
    using PedsFeature;
    using Scellecs.Morpeh;

    public interface IPainState {
        float PainThreshold { get; }
        string Color { get; }
        void ApplyPainIncreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed);
        void ApplyPainDecreased(SharedData sharedData, Entity pedEntity, ref ConvertedPed convertedPed);
        bool TryGetMoveSets(SharedData sharedData, bool isPlayer, out string[] moveSets);
    }
}