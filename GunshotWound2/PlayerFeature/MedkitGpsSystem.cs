namespace GunshotWound2.PlayerFeature {
    using System.Collections.Generic;
    using Configs;
    using GTA;
    using Scellecs.Morpeh;

    public sealed class MedkitGpsSystem : ILateSystem {
        private readonly SharedData sharedData;
        private readonly Model healthPackModel;
        private readonly HashSet<int> markedMedkits;
        private readonly List<(Prop kit, Blip blip)> medkits;
        private float timeToRefresh;

        public Scellecs.Morpeh.World World { get; set; }
        private PlayerConfig PlayerConfig => sharedData.mainConfig.PlayerConfig;

        public MedkitGpsSystem(SharedData sharedData) {
            this.sharedData = sharedData;

            healthPackModel = new Model(PlayerConfig.MedkitModel);
            markedMedkits = new HashSet<int>();
            medkits = new List<(Prop, Blip)>();
        }

        void IInitializer.OnAwake() { }

        public void OnUpdate(float deltaTime) {
            if (!PlayerConfig.BlipsToMedkits || !sharedData.TryGetPlayer(out _)) {
                return;
            }

            timeToRefresh -= deltaTime;
            if (timeToRefresh > 0f) {
                return;
            }

            ClearMedkits(false);
            foreach (Prop prop in GTA.World.GetAllPickupObjects()) {
                if (!markedMedkits.Contains(prop.Handle) && prop.Model.Equals(healthPackModel)) {
                    MarkMedkit(prop);
                }
            }

            timeToRefresh = PlayerConfig.TimeToRefreshMedkits;
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
            ClearMedkits(true);
        }
    }
}