// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using PedsFeature;
    using Utils;

    public sealed class MainConfig {
        public static readonly char[] Separator = { ';' };

        public readonly WoundConfig woundConfig;
        public readonly PedsConfig pedsConfig;
        public readonly PlayerConfig playerConfig;
        public readonly WeaponConfig weaponConfig;
        public readonly ArmorConfig armorConfig;
        public readonly InventoryConfig inventoryConfig;

        public InputListener.Scheme CheckSelfKey;
        public InputListener.Scheme CheckClosestKey;
        public InputListener.Scheme BandageSelfKey;
        public InputListener.Scheme BandageClosestKey;
        public InputListener.Scheme DeathKey;
        public InputListener.Scheme HealKey;
        public InputListener.Scheme HelmetKey;
        public InputListener.Scheme IncreaseRangeKey;
        public InputListener.Scheme DecreaseRangeKey;
        public InputListener.Scheme PauseKey;

        public string Language = "EN";
        public bool InfoMessages = true;
        public bool PedsMessages = true;
        public bool WoundsMessages = true;
        public bool CriticalMessages = true;

        public MainConfig() {
            playerConfig = new PlayerConfig();
            woundConfig = new WoundConfig();
            pedsConfig = new PedsConfig();
            weaponConfig = new WeaponConfig();
            armorConfig = new ArmorConfig();
            inventoryConfig = new InventoryConfig();
        }

        public void ApplyTo(Notifier notifier) {
            notifier.info.show = InfoMessages;
            notifier.peds.show = PedsMessages;
            notifier.wounds.show = WoundsMessages;
            notifier.critical.show = CriticalMessages;
        }

        public PainMoveSets GetPainMoveSetsFor(in ConvertedPed convertedPed) {
            if (convertedPed.isPlayer) {
                return playerConfig.PainMoveSets;
            } else if (convertedPed.isMale) {
                return pedsConfig.MalePainMoveSets;
            } else {
                return pedsConfig.FemalePainMoveSets;
            }
        }

        public override string ToString() {
            return $"{woundConfig}\n" + $"{pedsConfig}\n" + $"{playerConfig}";
        }

        public (bool success, string reason) TryToLoad(string scriptPath, ILogger logger) {
            string path = Path.ChangeExtension(scriptPath, ".xml");
            if (!File.Exists(path)) {
                return (false, "GSW Config was not found");
            }

            XElement doc;
            try {
                doc = XDocument.Load(path).Root;
            } catch (Exception e) {
                Console.WriteLine(e);
                return (false, $"Incorrect XML file at {path}:\n{e.Message}");
            }

            string section = null;
            try {
                section = nameof(PlayerConfig);
                playerConfig.FillFrom(doc);

                section = nameof(PedsConfig);
                pedsConfig.FillFrom(doc);

                section = nameof(WoundConfig);
                woundConfig.FillFrom(doc);

                section = nameof(WeaponConfig);
                weaponConfig.FillFrom(doc, logger);

                section = nameof(ArmorConfig);
                armorConfig.FillFrom(doc);

                section = nameof(InventoryConfig);
                inventoryConfig.FillFrom(doc);

                section = nameof(FillHotkeysFrom);
                FillHotkeysFrom(doc);

                section = nameof(FillNotifications);
                FillNotifications(doc);
            } catch (Exception e) {
                return (false, $"Failed loading of {section}:\n{e.Message}");
            }

            return (true, null);
        }

        private void FillHotkeysFrom(XElement doc) {
            XElement node = doc.Element("Hotkeys");
            if (node == null) {
                return;
            }

            CheckSelfKey = node.Element("CheckSelfKey").GetKeyScheme();
            CheckClosestKey = node.Element("CheckClosestKey").GetKeyScheme();
            BandageSelfKey = node.Element("BandageSelfKey").GetKeyScheme();
            BandageClosestKey = node.Element("BandageClosestKey").GetKeyScheme();
            DeathKey = node.Element("DeathKey").GetKeyScheme();
            HealKey = node.Element("HealKey").GetKeyScheme();
            HelmetKey = node.Element("GetHelmetKey").GetKeyScheme();
            IncreaseRangeKey = node.Element("IncreaseRangeKey").GetKeyScheme();
            DecreaseRangeKey = node.Element("DecreaseRangeKey").GetKeyScheme();
            PauseKey = node.Element("PauseKey").GetKeyScheme();
        }

        private void FillNotifications(XElement doc) {
            XElement node = doc.Element("Notifications");
            if (node == null) {
                return;
            }

            Language = node.Element("Language").GetString();
            InfoMessages = node.Element("Info").GetBool();
            PedsMessages = node.Element("OtherPeds").GetBool();
            WoundsMessages = node.Element("Wounds").GetBool();
            CriticalMessages = node.Element("Critical").GetBool();
        }
    }
}