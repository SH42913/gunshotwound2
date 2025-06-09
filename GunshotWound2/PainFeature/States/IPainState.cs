namespace GunshotWound2.PainFeature.States {
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public interface IPainState {
        float PainThreshold { get; }
        Notifier.Color Color { get; }

        void ApplyPainIncreased(Entity pedEntity, ref ConvertedPed convertedPed);
        void ApplyPainDecreased(Entity pedEntity, ref ConvertedPed convertedPed);
        bool TryGetMoveSets(in ConvertedPed convertedPed, out string[] moveSets);
        bool TryGetMoodSets(in ConvertedPed convertedPed, out string[] moodSets);
    }
}