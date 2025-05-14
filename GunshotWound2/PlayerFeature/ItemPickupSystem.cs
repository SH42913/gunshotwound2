namespace GunshotWound2.PlayerFeature {
    using System;
    using System.Collections.Generic;
    using GTA;
    using GTA.Native;
    using HealthFeature;
    using InventoryFeature;
    using PedsFeature;
    using Scellecs.Morpeh;

    public sealed class ItemPickupSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly HashSet<int> vehiclesWithoutLoadout;
        private Filter pedsWithInventory;
        private Filter justHealed;

        public Scellecs.Morpeh.World World { get; set; }

        public ItemPickupSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            vehiclesWithoutLoadout = new HashSet<int>();
        }

        public void OnAwake() {
            pedsWithInventory = World.Filter.With<ConvertedPed>().With<Inventory>();
            justHealed = pedsWithInventory.With<TotallyHealedEvent>();
        }

        public void OnUpdate(float deltaTime) {
            CheckMedkitPickup();
            CheckPedInSpecialVehicle();
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

        private void CheckPedInSpecialVehicle() {
            foreach (Scellecs.Morpeh.Entity entity in pedsWithInventory) {
                ref ConvertedPed convertedPed = ref entity.GetComponent<ConvertedPed>();
                Vehicle vehicle = convertedPed.thisPed.CurrentVehicle;
                if (vehicle == null || vehicle.ClassType != VehicleClass.Emergency) {
                    continue;
                }

                if (convertedPed.thisPed.IsInVehicle() && vehiclesWithoutLoadout.Add(vehicle.Handle)) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Ped {convertedPed.name} just entered vehicle {vehicle.DisplayName}");
#endif
                    entity.SetComponent(new AddItemRequest {
                        loadout = sharedData.mainConfig.inventoryConfig.EmergencyVehicleLoadout,
                    });
                }
            }

            vehiclesWithoutLoadout.RemoveWhere(x => Function.Call<int>(Hash.GET_ENTITY_TYPE, x) != 2);
        }

        void IDisposable.Dispose() { }
    }
}