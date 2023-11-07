namespace GunshotWound2.PainFeature.States {
    using Peds;
    using Scellecs.Morpeh;

    public interface IPainState {
        float PainThreshold { get; }
        string Color { get; }
        void ApplyState(Entity pedEntity, ref ConvertedPed convertedPed);
    }
}