namespace GunshotWound2.PlayerFeature {
    using System;
    using InventoryFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PlayerPainRecoverySystem : ILateSystem {
        private readonly SharedData sharedData;
        private bool wasInPain;
        private int messageHandle;
        private int remainingClicks;

        public EcsWorld World { get; set; }

        public PlayerPainRecoverySystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.PainkillersSelfKey, RegisterClick);
            remainingClicks = -1;
        }

        public void OnUpdate(float deltaTime) {
            if (!sharedData.TryGetPlayer(out EcsEntity entity)) {
                return;
            }

            ref Pain pain = ref entity.GetComponent<Pain>();
            bool tooMuchPain = pain.TooMuchPain();
            if (wasInPain == tooMuchPain) {
                return;
            }

            ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
            bool hasPainkillers = entity.GetComponent<Inventory>().Has(PainkillersItem.template);
            if (tooMuchPain && hasPainkillers && !convertedPed.hasSpineDamage) {
                remainingClicks = (int)(pain.Percent() * 10);
#if DEBUG
                sharedData.logger.WriteInfo($"Starting pain recovery, remaining clicks = {remainingClicks}");
#endif
                ShowMessage();
            } else {
                remainingClicks = -1;
            }

            wasInPain = tooMuchPain;
        }

        void IDisposable.Dispose() { }

        private void RegisterClick() {
            if (remainingClicks < 0) {
                return;
            }

            if (!sharedData.TryGetPlayer(out EcsEntity entity)) {
                return;
            }

            remainingClicks--;
            if (remainingClicks > 0) {
                ShowMessage();
#if DEBUG
                sharedData.logger.WriteInfo($"Pain recovery remaining clicks = {remainingClicks}");
#endif
                return;
            }

            if (!entity.GetComponent<Inventory>().Consume(PainkillersItem.template)) {
                return;
            }

            entity.SetComponent(new PainkillersEffect {
                rate = sharedData.mainConfig.inventoryConfig.PainkillersRate,
                remainingTime = sharedData.mainConfig.inventoryConfig.PainkillersDuration,
            });
#if DEBUG
            sharedData.logger.WriteInfo("Pain recovery success!");
#endif

            string message = sharedData.localeConfig.PainkillersSuccess;
            messageHandle = sharedData.notifier.ReplaceOne(message, blinking: true, messageHandle, Notifier.Color.GREEN);
        }

        private void ShowMessage() {
            if (remainingClicks <= 0) {
                return;
            }

            string message = sharedData.localeConfig.PainkillersQTE;
            string key = sharedData.mainConfig.PainkillersSelfKey.description;
            message = string.Format(message, key, remainingClicks.ToString());
            messageHandle = sharedData.notifier.ReplaceOne(message, blinking: true, messageHandle, Notifier.Color.YELLOW);
        }
    }
}