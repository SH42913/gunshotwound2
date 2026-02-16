// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System;
    using System.IO;
    using System.Xml.Linq;
    using Utils;

    public sealed class MainConfig {
        public interface IConfig {
            public string sectionName { get; }
            public void FillFrom(XDocument doc);
            public void Validate(MainConfig mainConfig, ILogger logger);
        }

        public const string WEIGHT_ATTRIBUTE_NAME = "Weight";
        public const float DAMAGE_MODIFIER = 0.1f;
        public static readonly char[] Separator = [';',];

        private const string NOTIFICATIONS_ROOT_NAME = "Notifications";
        private const string LANGUAGE_NODE_NAME = "Language";
        private const string EN_LOCALE_NAME = "EN";

        public readonly WoundConfig woundConfig;
        public readonly PedsConfig pedsConfig;
        public readonly PlayerConfig playerConfig;
        public readonly WeaponConfig weaponConfig;
        public readonly ArmorConfig armorConfig;
        public readonly InventoryConfig inventoryConfig;
        public readonly BodyPartConfig bodyPartConfig;
        public readonly TraumaConfig traumaConfig;

        public InputListener.Scheme CheckSelfKey;
        public InputListener.Scheme CheckClosestKey;
        public InputListener.Scheme BandagesSelfKey;
        public InputListener.Scheme BandagesClosestKey;
        public InputListener.Scheme PainkillersSelfKey;
        public InputListener.Scheme PainkillersClosestKey;
        public InputListener.Scheme DeathKey;
        public InputListener.Scheme HealKey;
        public InputListener.Scheme HelmetKey;
        public InputListener.Scheme PauseKey;

        public string Language = EN_LOCALE_NAME;
        public bool InfoMessages = true;
        public bool PedsMessages = true;
        public bool WoundsMessages = true;
        public bool CriticalMessages = true;

        public bool HelpTipsEnabled;
        public float HelpTipDuration;
        public float HelpTipMinInterval;
        public float HelpTipMaxInterval;

        public bool HitNotificationEnabled;
        public bool CantDetectWeaponNotificationEnabled;

        public int HelpTipDurationInMs => (int)(HelpTipDuration * 1000);

        private readonly IConfig[] configs;

        public MainConfig() {
            playerConfig = new PlayerConfig();
            woundConfig = new WoundConfig();
            pedsConfig = new PedsConfig();
            weaponConfig = new WeaponConfig();
            armorConfig = new ArmorConfig();
            inventoryConfig = new InventoryConfig();
            bodyPartConfig = new BodyPartConfig();
            traumaConfig = new TraumaConfig();

            // order is loading order
            configs = new IConfig[] {
                playerConfig,
                pedsConfig,
                inventoryConfig,
                woundConfig,
                traumaConfig,
                bodyPartConfig,
                armorConfig,
                weaponConfig,
            };
        }

        public void ApplyTo(Notifier notifier) {
            notifier.info.show = InfoMessages;
            notifier.peds.show = PedsMessages;
            notifier.wounds.show = WoundsMessages;
            notifier.critical.show = CriticalMessages;
        }

        public override string ToString() {
            return $"{woundConfig}\n" + $"{pedsConfig}\n" + $"{playerConfig}";
        }

        public (bool success, string reason, string trace) TryToLoad(string scriptPath) {
            string section = null;
            try {
                foreach (IConfig config in configs) {
                    section = config.sectionName;
                    config.FillFrom(LoadDocument(scriptPath, section));
                }

                section = nameof(FillHotkeysFrom);
                XDocument doc = LoadDocument(scriptPath, "KeyBinds.xml");
                FillHotkeysFrom(doc);

                section = nameof(FillNotifications);
                const string sectionName = "Notifications.xml";
                doc = LoadDocument(scriptPath, sectionName);
                FillNotifications(doc);
                UpdateLanguageFromGame(scriptPath, doc, sectionName);
            } catch (Exception e) {
                return (false, $"Failed loading of {section}:\n{e.Message}", e.StackTrace);
            }

            return (true, null, null);
        }

        private void UpdateLanguageFromGame(string scriptPath, XDocument doc, string sectionName) {
            if (Language != EN_LOCALE_NAME || GTA.Game.Language == GTA.Language.American) {
                return;
            }

            XElement languageNode = doc.Element(NOTIFICATIONS_ROOT_NAME)?.Element(LANGUAGE_NODE_NAME);
            if (languageNode == null) {
                return;
            }

            switch (GTA.Game.Language) {
                case GTA.Language.French:            Language = "FR"; break;
                case GTA.Language.German:            Language = "DE"; break;
                case GTA.Language.Spanish:           Language = "SPA"; break;
                case GTA.Language.Portuguese:        Language = "PT-BR"; break;
                case GTA.Language.Polish:            Language = "PL"; break;
                case GTA.Language.Russian:           Language = "RU"; break;
                case GTA.Language.Korean:            Language = "KR"; break;
                case GTA.Language.Japanese:          Language = "JP"; break;
                case GTA.Language.Chinese:           Language = "ZH-HK"; break;
                case GTA.Language.ChineseSimplified: Language = "ZH-CN"; break;
            }

            string nodeValue = languageNode.GetString();
            if (Language != nodeValue) {
                languageNode.SetAttributeValue("Value", Language);
                string path = GetPathForSection(scriptPath, sectionName);
                doc.Save(path);
            }
        }

        public void ValidateConfigs(ILogger logger) {
            foreach (IConfig config in configs) {
                config.Validate(this, logger);
            }
        }

        private static XDocument LoadDocument(string scriptPath, string sectionName) {
            string path = GetPathForSection(scriptPath, sectionName);
            if (!File.Exists(path)) {
                throw new Exception($"GSW Config was not found at {path}");
            }

            return XDocument.Load(path);
        }

        private void FillHotkeysFrom(XDocument doc) {
            XElement root = doc.Element("KeyBinds")!;
            CheckSelfKey = root.Element(nameof(CheckSelfKey)).GetKeyScheme();
            CheckClosestKey = root.Element(nameof(CheckClosestKey)).GetKeyScheme();
            BandagesSelfKey = root.Element(nameof(BandagesSelfKey)).GetKeyScheme();
            BandagesClosestKey = root.Element(nameof(BandagesClosestKey)).GetKeyScheme();
            PainkillersSelfKey = root.Element(nameof(PainkillersSelfKey)).GetKeyScheme();
            PainkillersClosestKey = root.Element(nameof(PainkillersClosestKey)).GetKeyScheme();
            DeathKey = root.Element(nameof(DeathKey)).GetKeyScheme();
            HealKey = root.Element(nameof(HealKey)).GetKeyScheme();
            HelmetKey = root.Element("GetHelmetKey").GetKeyScheme();
            PauseKey = root.Element(nameof(PauseKey)).GetKeyScheme();
        }

        private void FillNotifications(XDocument doc) {
            XElement node = doc.Element(NOTIFICATIONS_ROOT_NAME);
            if (node == null) {
                return;
            }

            Language = node.Element(LANGUAGE_NODE_NAME).GetString();
            InfoMessages = node.Element("Info").GetBool();
            PedsMessages = node.Element("OtherPeds").GetBool();
            WoundsMessages = node.Element("Wounds").GetBool();
            CriticalMessages = node.Element("Critical").GetBool();

            XElement helpNode = node.Element("HelpTips");
            HelpTipsEnabled = helpNode.GetBool("Enabled");
            HelpTipDuration = helpNode.GetFloat("TipDurationInSec");
            HelpTipMinInterval = helpNode.GetFloat("MinIntervalInSec");
            HelpTipMaxInterval = helpNode.GetFloat("MaxIntervalInSec");

            HitNotificationEnabled = node.Element("HitNotification").GetBool("Enabled");
            CantDetectWeaponNotificationEnabled = node.Element("CantDetectWeaponNotification").GetBool("Enabled");
        }

        private static string GetPathForSection(string scriptPath, string sectionName) {
            return Path.ChangeExtension(scriptPath, sectionName);
        }
    }
}