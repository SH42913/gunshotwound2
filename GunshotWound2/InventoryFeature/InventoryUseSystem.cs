namespace GunshotWound2.InventoryFeature {
    using System;
    using GTA;
    using GTA.Math;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class InventoryUseSystem : ISystem {
        private static int ERROR_POST;
        private static int SUCCESS_POST;

        private readonly SharedData sharedData;
        private Filter usages;
        private Filter requests;

        public EcsWorld World { get; set; }

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
            foreach (EcsEntity owner in usages) {
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
                    convertedPed.thisPed.Task.ClearSecondary();
                    sharedData.uiService.HideProgressIndicator();
                    owner.RemoveComponent<CurrentlyUsingItem>();
                }
            }
        }

        private void ProcessRequests() {
            foreach (EcsEntity owner in requests) {
                var request = owner.GetComponent<UseItemRequest>();
                owner.RemoveComponent<UseItemRequest>();

                bool success = TryProcessRequest(owner, request, out string message);
                bool isPlayer = owner.GetComponent<ConvertedPed>().isPlayer;
                if (isPlayer) {
                    if (success) {
                        ShowSuccess(message, blinking: false);

                        string progressString = !string.IsNullOrEmpty(request.item.progressDescriptionKey)
                                ? sharedData.localeConfig.GetTranslation(request.item.progressDescriptionKey)
                                : sharedData.localeConfig.AnyItemProgress;

                        sharedData.uiService.ShowProgressIndicator(progressString);
                    } else {
                        ShowError(message);
                    }
                }
            }
        }

        private bool TryProcessRequest(EcsEntity owner, UseItemRequest request, out string message) {
            if (owner.Has<CurrentlyUsingItem>()) {
#if DEBUG
                sharedData.logger.WriteInfo("Can't use item due hands are busy");
#endif
                message = sharedData.localeConfig.HandsAreBusy;
                return false;
            }

            ItemTemplate item = request.item;
            ref Inventory inventory = ref owner.GetComponent<Inventory>();
            if (!inventory.Has(item)) {
#if DEBUG
                sharedData.logger.WriteInfo($"{inventory.modelHash} doesn't have enough of {item.key}");
#endif
                string itemCountString = item.GetPluralTranslation(sharedData.localeConfig, count: 0);
                message = $"{sharedData.localeConfig.YourInventory} {itemCountString}";
                return false;
            }

            EcsEntity target = request.target ?? owner;
            if (item.startAction.Invoke(sharedData, owner, target, out message)) {
                ref ConvertedPed convertedOwner = ref owner.GetComponent<ConvertedPed>();
                ref ConvertedPed convertedTarget = ref target.GetComponent<ConvertedPed>();
                int durationInMs = item.duration.ConvertToMilliSec();

                CrClipAsset crClipAsset;
                var flags = AnimationFlags.Secondary;
                Ped ownerPed = convertedOwner.thisPed;
                if (owner == target) {
                    crClipAsset = new CrClipAsset(item.selfAnimation.dict, item.selfAnimation.name);
                    flags |= AnimationFlags.UpperBodyOnly;
                } else {
                    if (convertedTarget.isRagdoll) {
                        crClipAsset = new CrClipAsset(item.otherRagdollAnimation.dict, item.otherRagdollAnimation.name);
                    } else {
                        crClipAsset = new CrClipAsset(item.otherAnimation.dict, item.otherAnimation.name);
                        flags |= AnimationFlags.UpperBodyOnly;
                    }

                    if (convertedOwner.thisPed.CurrentVehicle == null) {
                        ownerPed.Task.TurnTo(convertedTarget.thisPed, durationInMs);
                        RotatePedToOther(ownerPed, convertedTarget.thisPed);
                    }
                }

                ownerPed.Task.PlayAnimation(crClipAsset,
                                            AnimationBlendDelta.NormalBlendIn,
                                            AnimationBlendDelta.NormalBlendOut,
                                            durationInMs,
                                            flags,
                                            startPhase: 0f);

                owner.SetComponent(new CurrentlyUsingItem {
                    itemTemplate = item,
                    target = target,
                    remainingTime = item.duration,
                });

#if DEBUG
                sharedData.logger.WriteInfo($"Success start {item.key} usage for {inventory.modelHash}");
#endif

                return true;
            } else {
#if DEBUG
                sharedData.logger.WriteInfo($"{inventory.modelHash} failed start condition of {item.key}");
#endif
                return false;
            }
        }

        private static void RotatePedToOther(Ped ped, Ped other) {
            Vector3 up = ped.UpVector;
            Vector3 direction = other.BelowPosition - ped.BelowPosition;
            ped.Quaternion = Quaternion.LookRotation(direction, up);
        }

        private void UpdateUsing(EcsEntity owner,
                                 in Inventory inventory,
                                 in CurrentlyUsingItem currentlyUsing,
                                 out bool removeProgress) {
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

        private void HandleFinish(EcsEntity owner, ref Inventory inventory, in CurrentlyUsingItem currentlyUsing) {
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