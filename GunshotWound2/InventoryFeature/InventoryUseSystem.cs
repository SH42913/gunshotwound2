namespace GunshotWound2.InventoryFeature {
    using System;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class InventoryUseSystem : ISystem {
        private static int ERROR_POST;
        private static int SUCCESS_POST;

        private readonly SharedData sharedData;
        private Filter usages;
        private Filter requests;

        public World World { get; set; }

        public InventoryUseSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            usages = World.Filter.With<ConvertedPed>().With<CurrentlyUsingItem>().With<Inventory>();
            requests = World.Filter.With<ConvertedPed>().With<UseItemRequest>().With<Inventory>();
        }

        public void OnUpdate(float deltaTime) {
            ProcessUsages(deltaTime);
            World.Commit();
            ProcessRequests();
        }

        void IDisposable.Dispose() { }

        private void ProcessUsages(float deltaTime) {
            foreach (Entity owner in usages) {
                ref Inventory inventory = ref owner.GetComponent<Inventory>();
                ref ConvertedPed convertedPed = ref owner.GetComponent<ConvertedPed>();
                ref CurrentlyUsingItem currentUsing = ref owner.GetComponent<CurrentlyUsingItem>();
                currentUsing.remainingTime -= deltaTime;

                bool removeProgress;
                if (!convertedPed.thisPed.Exists() || convertedPed.thisPed.IsDead) {
                    removeProgress = true;
                } else if (currentUsing.remainingTime > 0f) {
                    UpdateUsing(owner, inventory, currentUsing, out removeProgress);
                } else {
                    HandleFinish(owner, ref inventory, currentUsing);
                    removeProgress = true;
                }

                if (removeProgress) {
                    sharedData.uiService.HideProgressIndicator();
                    owner.RemoveComponent<CurrentlyUsingItem>();
                }
            }
        }

        private void ProcessRequests() {
            foreach (Entity owner in requests) {
                var request = owner.GetComponent<UseItemRequest>();
                owner.RemoveComponent<UseItemRequest>();

                bool success = TryProcessRequest(owner, request, out string message);
                bool isPlayer = owner.GetComponent<ConvertedPed>().isPlayer;
                if (isPlayer) {
                    if (success) {
                        ShowSuccess(message, blinking: false);

                        string progressString = sharedData.localeConfig.GetTranslation(request.item.progressDescriptionKey);
                        sharedData.uiService.ShowProgressIndicator(progressString);
                    } else {
                        ShowError(message);
                    }
                }
            }
        }

        private bool TryProcessRequest(Entity owner, UseItemRequest request, out string message) {
            if (owner.Has<CurrentlyUsingItem>()) {
#if DEBUG
                sharedData.logger.WriteInfo("Can't use item due hands are busy");
#endif
                message = sharedData.localeConfig.HandsAreBusy;
                return false;
            }

            ref Inventory inventory = ref owner.GetComponent<Inventory>();
            if (!inventory.Has(request.item)) {
#if DEBUG
                sharedData.logger.WriteInfo($"{inventory.modelHash} doesn't have enough of {request.item.key}");
#endif
                string itemCountString = request.item.GetPluralTranslation(sharedData.localeConfig, count: 0);
                message = $"{sharedData.localeConfig.YourInventory} {itemCountString}";
                return false;
            }

            Entity target = request.target ?? owner;
            if (request.item.startAction.Invoke(sharedData, owner, target, out message)) {
                owner.SetComponent(new CurrentlyUsingItem {
                    itemTemplate = request.item,
                    target = target,
                    remainingTime = request.item.duration,
                });

#if DEBUG
                sharedData.logger.WriteInfo($"Success start {request.item.key} usage for {inventory.modelHash}");
#endif

                return true;
            } else {
#if DEBUG
                sharedData.logger.WriteInfo($"{inventory.modelHash} failed start condition of {request.item.key}");
#endif
                return false;
            }
        }

        private void UpdateUsing(Entity owner, in Inventory inventory, in CurrentlyUsingItem currentlyUsing, out bool removeProgress) {
            ItemTemplate item = currentlyUsing.itemTemplate;
            if (item.progressAction.Invoke(sharedData, owner, currentlyUsing.target, out string message)) {
                removeProgress = false;
                ShowSuccess(message, blinking: false);
                return;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Item {item.key} of {inventory.modelHash} was canceled during progress");
#endif
            ShowError(message);
            removeProgress = true;
        }

        private void HandleFinish(Entity owner, ref Inventory inventory, in CurrentlyUsingItem currentlyUsing) {
            ItemTemplate item = currentlyUsing.itemTemplate;
            if (item.finishAction.Invoke(sharedData, owner, currentlyUsing.target, out string message) && inventory.Consume(item)) {
#if DEBUG
                int amount = inventory.AmountOf(item);
                sharedData.logger.WriteInfo($"Item {item.key} of {inventory.modelHash} successfully used, amount={amount}");
#endif
                ShowSuccess(message, blinking: true);
            } else {
#if DEBUG
                sharedData.logger.WriteInfo($"Item {item.key} of {inventory.modelHash} usage was failed during finish");
#endif
                ShowError(message);
            }
        }

        private void ShowError(string message) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }

            sharedData.notifier.HideOne(SUCCESS_POST);
            ERROR_POST = sharedData.notifier.ReplaceOne(message, blinking: true, ERROR_POST, Notifier.Color.RED);
        }

        private void ShowSuccess(string message, bool blinking) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }

            SUCCESS_POST = sharedData.notifier.ReplaceOne(message, blinking, SUCCESS_POST, Notifier.Color.GREEN);
        }
    }
}