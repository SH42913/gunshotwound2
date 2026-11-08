namespace GunshotWound2.TraumaFeature {
    using Configs;
    using PedsFeature;

    public abstract class BaseTraumaEffect {
        protected readonly SharedData sharedData;

        public abstract string PlayerMessage { get; }
        public abstract string ManMessage { get; }
        public abstract string WomanMessage { get; }

        protected BaseTraumaEffect(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public abstract void Apply(EcsEntity entity, in BodyPartConfig.BodyPart bodyPart, ref ConvertedPed convertedPed);
        public abstract void Repeat(EcsEntity entity, ref ConvertedPed convertedPed);
        public abstract void EveryFrame(EcsEntity entity, ref ConvertedPed convertedPed);
        public abstract void Cancel(EcsEntity entity, ref ConvertedPed convertedPed);

        protected void ShowRunningWarningMessage(in ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                string message = sharedData.localeConfig.RunningWithScissors;
                sharedData.notifier.critical.QueueMessage(message);
            }
        }
    }
}