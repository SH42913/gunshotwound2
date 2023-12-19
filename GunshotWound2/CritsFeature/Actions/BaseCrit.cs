namespace GunshotWound2.CritsFeature {
    using HealthFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public abstract class BaseCrit {
        protected readonly SharedData sharedData;

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
                sharedData.notifier.warning.AddMessage(PlayerMessage);
            } else if (sharedData.mainConfig.NpcConfig.ShowEnemyCriticalMessages) {
                sharedData.notifier.info.AddMessage(convertedPed.isMale ? ManMessage : WomanMessage);
            }
        }

        public abstract void Apply(Entity pedEntity, ref ConvertedPed convertedPed);
        public abstract void Cancel(Entity pedEntity, ref ConvertedPed convertedPed);

        protected void CreatePain(Entity entity, float amount) {
            entity.GetComponent<Pain>().diff += amount;
        }

        protected void CreateInternalBleeding(Entity pedEntity, float severity) {
            pedEntity.CreateBleeding(severity, sharedData.localeConfig.InternalBleeding, isInternal: true);
        }
    }
}