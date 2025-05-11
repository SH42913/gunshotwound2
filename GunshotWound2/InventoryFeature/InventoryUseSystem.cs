namespace GunshotWound2.InventoryFeature {
    using System;
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
            usages = World.Filter.With<CurrentlyUsingItem>().With<Inventory>();
            requests = World.Filter.With<UseItemRequest>().With<Inventory>();
        }

        public void OnUpdate(float deltaTime) {
            ProcessUsages(deltaTime);
            World.Commit();
            ProcessRequests();
        }

        void IDisposable.Dispose() { }

        private void ProcessUsages(float deltaTime) {
            foreach (Entity owner in usages) {
                ref CurrentlyUsingItem currentUsing = ref owner.GetComponent<CurrentlyUsingItem>();
                ref Inventory inventory = ref owner.GetComponent<Inventory>();

                currentUsing.remainingTime -= deltaTime;
                bool removeProgress;
                if (currentUsing.remainingTime > 0f) {
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
                if (owner.Has<CurrentlyUsingItem>()) {
#if DEBUG
                    sharedData.logger.WriteInfo("Can't use item due hands are busy");
#endif
                    ShowError("Your hands are busy"); // TODO: Localize
                    continue;
                }

                ref UseItemRequest request = ref owner.GetComponent<UseItemRequest>();
                ref Inventory inventory = ref owner.GetComponent<Inventory>();
                if (!inventory.Has(request.item)) {
#if DEBUG
                    sharedData.logger.WriteInfo($"{inventory.modelHash} doesn't have enough of {request.item.internalName}");
#endif
                    ShowError($"In your inventory: 0 {request.item.internalName}"); // TODO: Localize
                    continue;
                }

                Entity target = request.target ?? owner;
                if (request.item.startAction.Invoke(sharedData, owner, target, out string message)) {
                    owner.SetComponent(new CurrentlyUsingItem {
                        itemTemplate = request.item,
                        target = target,
                        remainingTime = request.item.duration,
                    });

#if DEBUG
                    sharedData.logger.WriteInfo($"Success start {request.item.internalName} usage for {inventory.modelHash}");
#endif

                    sharedData.uiService.ShowProgressIndicator(request.item.progressDescriptionKey); // TODO: Localize
                    owner.RemoveComponent<UseItemRequest>();
                    ShowSuccess(message);
                } else {
#if DEBUG
                    sharedData.logger.WriteInfo($"{inventory.modelHash} failed start condition of {request.item.internalName}");
#endif
                    ShowError(message);
                }
            }
        }

        private void UpdateUsing(Entity owner, in Inventory inventory, in CurrentlyUsingItem currentlyUsing, out bool removeProgress) {
            ItemTemplate item = currentlyUsing.itemTemplate;
            if (item.progressAction.Invoke(sharedData, owner, currentlyUsing.target, out string message)) {
                removeProgress = false;
                ShowSuccess(message);
                return;
            }

#if DEBUG
            sharedData.logger.WriteInfo($"Item {item.internalName} of {inventory.modelHash} was canceled during progress");
#endif
            ShowError(message);
            removeProgress = true;
        }

        private void HandleFinish(Entity owner, ref Inventory inventory, in CurrentlyUsingItem currentlyUsing) {
            ItemTemplate item = currentlyUsing.itemTemplate;
            if (item.finishAction.Invoke(sharedData, owner, currentlyUsing.target, out string message) && inventory.Consume(item)) {
#if DEBUG
                int amount = inventory.AmountOf(item);
                sharedData.logger.WriteInfo($"Item {item.internalName} of {inventory.modelHash} successfully used, amount={amount}");
#endif
                ShowSuccess(message);
            } else {
#if DEBUG
                sharedData.logger.WriteInfo($"Item {item.internalName} of {inventory.modelHash} usage was failed during finish");
#endif
                ShowError(message);
            }
        }

        private void ShowError(string message) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }

            ERROR_POST = sharedData.notifier.ReplaceOne(message, blinking: true, ERROR_POST, Notifier.Color.RED);
        }

        private void ShowSuccess(string message) {
            if (string.IsNullOrEmpty(message)) {
                return;
            }

            SUCCESS_POST = sharedData.notifier.ReplaceOne(message, blinking: false, SUCCESS_POST, Notifier.Color.GREEN);
        }
    }
}