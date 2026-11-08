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
        private AspectFactory<PedWithInventory> pedWithInventoryAspect;

        public EcsWorld World { get; set; }

        public ItemPickupSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            vehiclesWithoutLoadout = new HashSet<int>();
        }

        public void OnAwake() {
            pedsWithInventory = World.Filter.Extend<PedWithInventory>();
            justHealed = World.Filter.Extend<PedWithInventory>().With<TotallyHealedEvent>();
            pedWithInventoryAspect = World.GetAspectFactory<PedWithInventory>();
        }

        public void OnUpdate(float deltaTime) {
            CheckMedkitPickup();
            CheckPedInSpecialVehicle();
        }

        private void CheckMedkitPickup() {
            foreach (EcsEntity entity in justHealed) {
                const float medkitRange = 2f;

                ref ConvertedPed convertedPed = ref pedWithInventoryAspect.Get(entity).Ped;
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
                            items = sharedData.mainConfig.inventoryConfig.MedkitLoadout.items,
                        });

                        break;
                    }
                }
            }
        }

        private void CheckPedInSpecialVehicle() {
            foreach (EcsEntity entity in pedsWithInventory) {
                ref ConvertedPed convertedPed = ref pedWithInventoryAspect.Get(entity).Ped;
                if (convertedPed.isPlayer && !Game.Player.CanControlCharacter) {
                    continue;
                }

                if (convertedPed.IsInEmergencyVehicle(out Vehicle vehicle) && vehiclesWithoutLoadout.Add(vehicle.Handle)) {
#if DEBUG
                    sharedData.logger.WriteInfo($"Ped {convertedPed.name} just entered vehicle {vehicle.DisplayName}");
#endif
                    entity.SetComponent(new AddItemRequest {
                        items = sharedData.mainConfig.inventoryConfig.EmergencyVehicleLoadout.items,
                    });
                }
            }

            vehiclesWithoutLoadout.RemoveWhere(x => Function.Call<int>(Hash.GET_ENTITY_TYPE, x) != 2);
        }

        void IDisposable.Dispose() { }
    }
}