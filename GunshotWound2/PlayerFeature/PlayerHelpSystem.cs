// #define DEBUG_EVERY_FRAME

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
        private readonly SharedData sharedData;
        private bool bandageHelpShown;
        private bool painkillerHelpShown;
        private bool emptyInventoryHelpShown;
        private bool traumaHelpShown;
        private bool deathKeyHelpShown;
        private float timeToNextHelp;

        public EcsWorld World { get; set; }

        private MainConfig MainConfig => sharedData.mainConfig;
        private LocaleConfig LocaleData => sharedData.localeConfig;

        public PlayerHelpSystem(SharedData sharedData) {
            this.sharedData = sharedData;
        }

        public void OnAwake() {
            RefreshTimeToNextHelp();
        }

        public void OnUpdate(float deltaTime) {
            if (!MainConfig.HelpTipsEnabled || Screen.IsHelpTextDisplayed) {
                return;
            }

            if (!sharedData.PlayerIsInitialized() || !sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
                return;
            }

            ref Health health = ref playerEntity.GetComponent<Health>();
            ref Inventory inventory = ref playerEntity.GetComponent<Inventory>();
            bool hasBandages = inventory.Has(BandageItem.template);
            if (hasBandages && !bandageHelpShown && TryShowBandageHelp(health)) {
                bandageHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            ref Pain pain = ref playerEntity.GetComponent<Pain>();
            bool hasPainkillers = inventory.Has(PainkillersItem.template);
            if (hasPainkillers && !painkillerHelpShown && TryShowPainkillersHelp(ref pain)) {
                painkillerHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            bool hasSpineDamage = playerEntity.GetComponent<ConvertedPed>().hasSpineDamage;
            if (!deathKeyHelpShown && TryShowDeathKeyHelp(ref pain, hasPainkillers, hasSpineDamage)) {
                deathKeyHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            if (!hasSpineDamage && !traumaHelpShown && TryShowTraumaHelp(health)) {
                traumaHelpShown = true;
                RefreshTimeToNextHelp();
                return;
            }

            if ((!hasBandages || !hasPainkillers) && !emptyInventoryHelpShown) {
                emptyInventoryHelpShown = true;
                ShowHelpTip(LocaleData.MedkitsHelpMessage);
                RefreshTimeToNextHelp();
                return;
            }

            timeToNextHelp -= deltaTime;
#if DEBUG && DEBUG_EVERY_FRAME
            sharedData.logger.WriteInfo($"Time to help tip:{timeToNextHelp}");
#endif
            if (timeToNextHelp > 0f) {
                return;
            }

            RefreshTimeToNextHelp();

            IWeightedRandomizer<int> weightRandom = sharedData.weightRandom;
            weightRandom.Clear();
            weightRandom.Add(0);
            weightRandom.Add(1);
            weightRandom.Add(2);

            if (sharedData.mainConfig.inventoryConfig.BlipsToMedkits) {
                weightRandom.Add(3);
            }

            switch (weightRandom.NextWithRemoval()) {
                case 0: ShowCheckHelp(); break;
                case 1: ShowHelpTip(LocaleData.MedkitsHelpMessage); break;
                case 2: ShowClosestPedsHelp(); break;
                case 3: ShowHelpTip(LocaleData.MedkitsOnMapHelpMessage); break;
            }
        }

        private bool TryShowTraumaHelp(in Health health) {
            if (!health.HasBleedingWounds()) {
                return false;
            }

            foreach (EcsEntity entity in health.bleedingWounds) {
                if (entity.GetComponent<Bleeding>().isTrauma) {
                    ShowHelpTip(LocaleData.TraumaHelpMessage);
                    return true;
                }
            }

            return false;
        }

        private bool TryShowBandageHelp(in Health health) {
            if (health.bleedingToBandage.IsNullOrDisposed()) {
                return false;
            } else {
                string key = WrapKey(MainConfig.BandagesSelfKey);
                string message = string.Format(LocaleData.BandageHelpMessage, key);
                ShowHelpTip(message);
                return true;
            }
        }

        private bool TryShowPainkillersHelp(ref Pain pain) {
            if (pain.Percent() > 0.8f) {
                string key = WrapKey(MainConfig.PainkillersSelfKey);
                string message = string.Format(LocaleData.PainkillerHelpMessage, key);
                ShowHelpTip(message);
                return true;
            } else {
                return false;
            }
        }

        private bool TryShowDeathKeyHelp(ref Pain pain, bool hasPainkillers, bool hasSpineDamage) {
            if (pain.TooMuchPain() && !hasPainkillers) {
                Show();
                return true;
            } else if (hasSpineDamage) {
                Show();
                return true;
            } else {
                return false;
            }

            void Show() {
                string key = WrapKey(MainConfig.DeathKey);
                string message = string.Format(LocaleData.DeathKeyHelpMessage, key);
                ShowHelpTip(message);
            }
        }

        private void ShowCheckHelp() {
            string key = WrapKey(MainConfig.CheckSelfKey);
            string message = string.Format(LocaleData.CheckHelpMessage, key);
            ShowHelpTip(message);
        }

        private void ShowClosestPedsHelp() {
            // ReSharper disable once RedundantExplicitParamsArrayCreation
            string keys = string.Join(", ",
                                      new[] {
                                          WrapKey(MainConfig.CheckClosestKey),
                                          WrapKey(MainConfig.BandagesClosestKey),
                                          WrapKey(MainConfig.PainkillersClosestKey),
                                      });

            string message = string.Format(LocaleData.ClosestPedHelpMessage, keys);
            ShowHelpTip(message);
        }

        private void RefreshTimeToNextHelp() {
            timeToNextHelp = sharedData.random.NextFloat(MainConfig.HelpTipMinInterval, MainConfig.HelpTipMaxInterval);
        }

        private void ShowHelpTip(string message) {
            Screen.ShowHelpText(message, MainConfig.HelpTipDurationInMs);
        }

        public static string WrapKey(in InputListener.Scheme key) {
            string description = key.description;
            return $"~h~[{description}]~h~";
        }

        void IDisposable.Dispose() { }
    }
}