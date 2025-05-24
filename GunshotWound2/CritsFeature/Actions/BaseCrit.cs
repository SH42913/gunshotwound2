namespace GunshotWound2.CritsFeature {
    using Configs;
    using HealthFeature;
    using HitDetection;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public abstract class BaseCrit {
        protected readonly SharedData sharedData;

        protected abstract string CritName { get; }
        protected abstract string PlayerMessage { get; }
        protected abstract string ManMessage { get; }
        protected abstract string WomanMessage { get; }

        protected BaseCrit(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void ShowCritMessage(ref ConvertedPed convertedPed) {
            if (convertedPed.hasSpineDamage) {
                return;
            }

            if (convertedPed.isPlayer) {
                sharedData.notifier.critical.QueueMessage(PlayerMessage, Notifier.Color.YELLOW);
            } else {
                sharedData.notifier.peds.QueueMessage(convertedPed.isMale ? ManMessage : WomanMessage);
            }
        }

        public abstract void Apply(Entity pedEntity, ref ConvertedPed convertedPed);
        public abstract void EveryFrame(Entity pedEntity, ref ConvertedPed convertedPed);
        public abstract void Cancel(Entity pedEntity, ref ConvertedPed convertedPed);

        protected void CreatePain(Entity entity, float amount) {
            entity.GetComponent<Pain>().diff += amount;
        }

        protected void CreateInternalBleeding(Entity pedEntity, BodyPartConfig.BodyPart bodyPart, float severity) {
            pedEntity.CreateBleeding(bodyPart, severity, sharedData.localeConfig.InternalBleeding, isInternal: true);
        }

        protected void ShowRunningWarningMessage(in ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                string message = sharedData.localeConfig.RunningWithScissors;
                message = string.Format(message, CritName);
                sharedData.notifier.critical.QueueMessage(message);
            }
        }
    }
}