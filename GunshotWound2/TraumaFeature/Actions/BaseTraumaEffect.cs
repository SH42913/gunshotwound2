namespace GunshotWound2.TraumaFeature {
    using Configs;
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public abstract class BaseTraumaEffect {
        protected readonly SharedData sharedData;

        public abstract string PlayerMessage { get; }
        public abstract string ManMessage { get; }
        public abstract string WomanMessage { get; }

        protected BaseTraumaEffect(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public abstract void Apply(Entity entity, ref ConvertedPed convertedPed);
        public abstract void EveryFrame(Entity entity, ref ConvertedPed convertedPed);
        public abstract void Cancel(Entity entity, ref ConvertedPed convertedPed);

        protected void ShowRunningWarningMessage(in ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                string message = sharedData.localeConfig.RunningWithScissors;
                sharedData.notifier.critical.QueueMessage(message);
            }
        }
    }
}