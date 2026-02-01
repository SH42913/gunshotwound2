namespace GunshotWound2.StatusFeature {
    using PedsFeature;
    using Utils;

    public interface IPedStatus {
        string LocKey { get; }

        float PainThreshold { get; }
        float HealthThreshold { get; }

        Notifier.Color Color { get; }

        float MoveRate { get; }
        string[] MaleMoveSets { get; }
        string[] FemaleMoveSets { get; }
        string[] FacialIdleAnims { get; }
        string[] PlayerSpeechSet { get; }
        string[] PedSpeechSet { get; }

        void ApplyStatusTo(ref ConvertedPed convertedPed);
        void RemoveStatusFrom(ref ConvertedPed convertedPed);
    }
}