namespace GunshotWound2.PlayerFeature {
    using System;
    using Configs;
    using GTA.UI;
    using HealthFeature;
    using InventoryFeature;
    using PainFeature;
    using PedsFeature;
    using Scellecs.Morpeh;
    using Utils;
    using Weighted_Randomizer;
    using EcsEntity = Scellecs.Morpeh.Entity;
    using EcsWorld = Scellecs.Morpeh.World;

    public sealed class PlayerHelpSystem : ILateSystem {
        public const int HELP_DURATION = 5000;

        private readonly SharedData sharedData;
        private bool bandageHelpShown;
        private bool painkillerHelpShown;
        private bool deathKeyHelpShown;
        private float timeToNextHelp;

        public EcsWorld World { get; set; }

        public PlayerHelpSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            RefreshTimeToNextHelp();
        }

        public void OnUpdate(float deltaTime) {
            if (Screen.IsHelpTextDisplayed) {
                return;
            }

            if (!sharedData.PlayerIsInitialized() || !sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                return;
            }

            ref Inventory inventory = ref playerEntity.GetComponent<Inventory>();
            bool hasBandages = inventory.Has(BandageItem.template);
            if (hasBandages && !bandageHelpShown && TryShowBandageHelp(playerEntity)) {
                bandageHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            bool hasPainkillers = inventory.Has(PainkillersItem.template);
            if (hasPainkillers && !painkillerHelpShown && TryShowPainkillersHelp(playerEntity)) {
                painkillerHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            if (!deathKeyHelpShown && TryShowDeathKeyHelp(playerEntity, hasPainkillers)) {
                deathKeyHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            timeToNextHelp -= deltaTime;
            if (timeToNextHelp > 0f) {
                return;
            }

            RefreshTimeToNextHelp();

            IWeightedRandomizer<int> weightRandom = sharedData.weightRandom;
            weightRandom.Clear();
            weightRandom.Add(0);
            weightRandom.Add(1);
            weightRandom.Add(2);

            switch (weightRandom.NextWithRemoval()) {
                case 0: ShowCheckHelp(); break;
                case 1: ShowHelpTip(sharedData.localeConfig.MedkitsHelpMessage); break;
                case 2: ShowClosestPedsHelp(); break;
            }
        }

        private bool TryShowBandageHelp(EcsEntity playerEntity) {
            ref Health health = ref playerEntity.GetComponent<Health>();
            if (health.bleedingToBandage.IsNullOrDisposed()) {
                return false;
            } else {
                string key = WrapKey(sharedData.mainConfig.BandagesSelfKey);
                string message = string.Format(sharedData.localeConfig.BandageHelpMessage, key);
                ShowHelpTip(message);
                return true;
            }
        }

        private bool TryShowPainkillersHelp(EcsEntity playerEntity) {
            ref Pain pain = ref playerEntity.GetComponent<Pain>();
            if (pain.Percent() > 0.8f) {
                string key = WrapKey(sharedData.mainConfig.PainkillersSelfKey);
                string message = string.Format(sharedData.localeConfig.PainkillerHelpMessage, key);
                ShowHelpTip(message);
                return true;
            } else {
                return false;
            }
        }

        private bool TryShowDeathKeyHelp(EcsEntity playerEntity, bool hasPainkillers) {
            ref Pain pain = ref playerEntity.GetComponent<Pain>();
            if (pain.TooMuchPain() && !hasPainkillers) {
                Show();
                return true;
            } else if (playerEntity.GetComponent<ConvertedPed>().hasSpineDamage) {
                Show();
                return true;
            } else {
                return false;
            }

            void Show() {
                string key = WrapKey(sharedData.mainConfig.DeathKey);
                string message = string.Format(sharedData.localeConfig.DeathKeyHelpMessage, key);
                ShowHelpTip(message);
            }
        }

        private void ShowCheckHelp() {
            string key = WrapKey(sharedData.mainConfig.CheckSelfKey);
            string message = string.Format(sharedData.localeConfig.CheckHelpMessage, key);
            ShowHelpTip(message);
        }

        private void ShowClosestPedsHelp() {
            MainConfig mainConfig = sharedData.mainConfig;

            // ReSharper disable once RedundantExplicitParamsArrayCreation
            string keys = string.Join(", ",
                                      new[] {
                                          WrapKey(mainConfig.CheckClosestKey),
                                          WrapKey(mainConfig.BandagesClosestKey),
                                          WrapKey(mainConfig.PainkillersClosestKey),
                                      });

            string message = string.Format(sharedData.localeConfig.ClosestPedHelpMessage, keys);
            ShowHelpTip(message);
        }

        private void RefreshTimeToNextHelp() {
            timeToNextHelp = sharedData.random.NextFloat(60f, 60f * 3f);
        }

        private static void ShowHelpTip(string message) {
            Screen.ShowHelpText(message, HELP_DURATION);
        }

        public static string WrapKey(in InputListener.Scheme key) {
            string description = key.description;
            return $"~h~{description}~h~";
        }

        void IDisposable.Dispose() { }
    }
}