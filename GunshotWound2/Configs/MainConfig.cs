// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.IO;
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
        public readonly BodyPartConfig bodyPartConfig;

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
            bodyPartConfig = new BodyPartConfig();
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
            string section = null;
            try {
                section = nameof(PlayerConfig);
                XDocument doc = LoadDocument(scriptPath, "Player.xml");
                playerConfig.FillFrom(doc);

                section = nameof(PedsConfig);
                doc = LoadDocument(scriptPath, "Peds.xml");
                pedsConfig.FillFrom(doc);

                section = nameof(WoundConfig);
                doc = LoadDocument(scriptPath, "Wounds.xml");
                woundConfig.FillFrom(doc);

                section = nameof(WeaponConfig);
                doc = LoadDocument(scriptPath, "Weapons.xml");
                weaponConfig.FillFrom(doc, logger);

                section = nameof(ArmorConfig);
                doc = LoadDocument(scriptPath, "Armor.xml");
                armorConfig.FillFrom(doc);

                section = nameof(InventoryConfig);
                doc = LoadDocument(scriptPath, "Inventory.xml");
                inventoryConfig.FillFrom(doc);

                section = nameof(BodyPartConfig);
                doc = LoadDocument(scriptPath, "BodyParts.xml");
                bodyPartConfig.FillFrom(doc);

                section = nameof(FillHotkeysFrom);
                doc = LoadDocument(scriptPath, "KeyBinds.xml");
                FillHotkeysFrom(doc);

                section = nameof(FillNotifications);
                doc = LoadDocument(scriptPath, "Notifications.xml");
                FillNotifications(doc);
            } catch (Exception e) {
                return (false, $"Failed loading of {section}:\n{e.Message}");
            }

            return (true, null);
        }

        private static XDocument LoadDocument(string scriptPath, string sectionName) {
            string path = Path.ChangeExtension(scriptPath, sectionName);
            if (!File.Exists(path)) {
                throw new Exception($"GSW Config was not found at {path}");
            }

            return XDocument.Load(path);
        }

        private void FillHotkeysFrom(XDocument doc) {
            XElement node = doc.Element("KeyBinds");
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

        private void FillNotifications(XDocument doc) {
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