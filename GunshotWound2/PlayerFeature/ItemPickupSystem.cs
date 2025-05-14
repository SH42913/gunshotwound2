namespace GunshotWound2.PlayerFeature {
    using System;
    using System.Collections.Generic;
    using HealthFeature;
    using InventoryFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class ItemPickupSystem : ILateSystem {
        private readonly SharedData sharedData;
        private Filter justHealed;

        public Scellecs.Morpeh.World World { get; set; }

        public ItemPickupSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            justHealed = World.Filter.With<ConvertedPed>().With<Inventory>().With<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            CheckMedkitPickup();
        }

        private void CheckMedkitPickup() {
            foreach (Scellecs.Morpeh.Entity entity in justHealed) {
                const float medkitRange = 2f;

                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                Ped ped = convertedPed.thisPed;
                IReadOnlyList<PickupObject> pickups = sharedData.worldService.GetAllPickupObjects();

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < pickups.Count; i++) {
                    PickupObject pickup = pickups[i];
                    if (sharedData.modelChecker.IsMedkit(pickup.Model) && ped.IsInRange(pickup.Position, medkitRange)) {
#if DEBUG
                        sharedData.logger.WriteInfo($"Ped {convertedPed.name} just picked up medkit");
#endif

                        entity.SetComponent(new AddItemRequest {
                            loadout = sharedData.mainConfig.inventoryConfig.MedkitLoadout,
                        });

                        break;
                    }
                }
            }
        }

        void IDisposable.Dispose() { }
    }
}