namespace GunshotWound2.PlayerFeature {
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using HealthFeature;
    using InventoryFeature;
    using Scellecs.Morpeh;
    using TraumaFeature;
    using Utils;

    public sealed class MedkitGpsSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly HashSet<int> markedMedkits;
        private readonly List<(Prop kit, Blip blip)> medkits;
        private float timeToRefresh;

        public Scellecs.Morpeh.World World { get; set; }
        private InventoryConfig InventoryConfig => sharedData.mainConfig.inventoryConfig;

        public MedkitGpsSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            markedMedkits = new HashSet<int>();
            medkits = new List<(Prop, Blip)>();
        }

        void IInitializer.OnAwake() { }

        public void OnUpdate(float deltaTime) {
            if (!InventoryConfig.BlipsToMedkits || !sharedData.TryGetPlayer(out _)) {
                return;
            }

            timeToRefresh -= deltaTime;
            if (timeToRefresh > 0f) {
                return;
            }

            ClearMedkits(removeAll: false);
            MarkNewMedkits();
            FlashBlipsIfNeed();
            timeToRefresh = InventoryConfig.TimeToRefreshMedkits;
        }

        private void FlashBlipsIfNeed() {
            bool hasTraumas = sharedData.playerEntity.Has<Traumas>();
            bool hasBandages = sharedData.playerEntity.GetComponent<Inventory>().Has(BandageItem.template);
            if (!hasTraumas && hasBandages) {
                return;
            }

            foreach ((Prop _, Blip blip) in medkits) {
                blip.IsFlashing = true;
                blip.FlashTimeLeft = InventoryConfig.TimeToRefreshMedkits.ConvertToMilliSec();
            }
        }

        private void MarkNewMedkits() {
            foreach (PickupObject pickup in sharedData.worldService.GetAllPickupObjects()) {
                if (!markedMedkits.Contains(pickup.Handle) && sharedData.modelChecker.IsMedkit(pickup.Model)) {
                    MarkMedkit(pickup);
                }
            }
        }

        private void MarkMedkit(Prop prop) {
            Blip blip = prop.AddBlip();
            blip.Sprite = BlipSprite.Health;
            blip.Color = BlipColor.Green;
            blip.Scale = 0.5f;

            markedMedkits.Add(prop.Handle);
            medkits.Add((prop, blip));
        }

        private void ClearMedkits(bool removeAll) {
            for (int i = medkits.Count - 1; i >= 0; i--) {
                (Prop kit, Blip blip) = medkits[i];
                if (removeAll || !kit.Exists()) {
                    blip.Delete();
                    markedMedkits.Remove(kit.Handle);
                    medkits.RemoveAt(i);
                }
            }
        }

        public void Dispose() {
            ClearMedkits(removeAll: true);
        }
    }
}