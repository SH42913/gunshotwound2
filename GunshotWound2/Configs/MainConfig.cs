﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using GunshotWound2.Utils;

namespace GunshotWound2.Configs
{
    public sealed class MainConfig
    {
        public const float MINIMAL_RANGE_FOR_WOUNDED_PEDS = 0;
        public const float ADDING_TO_REMOVING_MULTIPLIER = 2;

        private const string ScriptConfigPath = "scripts/GSW2Config.xml";
        private const string GswConfigPath = "scripts/GSW2/GSW2Config.xml";
        private static readonly char[] Separator = {';'};

        public WoundConfig WoundConfig;
        public NpcConfig NpcConfig;
        public PlayerConfig PlayerConfig;

        public string Language = "EN";

        public Keys? HelmetKey;
        public Keys? CheckKey;
        public Keys? HealKey;
        public Keys? IncreaseRangeKey;
        public Keys? ReduceRangeKey;
        public Keys? PauseKey;
        public Keys? BandageKey;

        public bool CommonMessages = true;
        public bool WarningMessages = true;
        public bool AlertMessages = true;
        public bool EmergencyMessages = true;

        public HashSet<uint> SmallCaliberHashes;
        public HashSet<uint> MediumCaliberHashes;
        public HashSet<uint> HeavyCaliberHashes;
        public HashSet<uint> LightImpactHashes;
        public HashSet<uint> HeavyImpactHashes;
        public HashSet<uint> ShotgunHashes;
        public HashSet<uint> CuttingHashes;
        public HashSet<uint> IgnoreHashes;

        public void ApplyTo(Notifier notifier) {
            notifier.info.show = CommonMessages;
            notifier.warning.show = WarningMessages;
            notifier.alert.show = AlertMessages;
            notifier.emergency.show = EmergencyMessages;
        }

        public override string ToString()
        {
            return $"{WoundConfig}\n" +
                   $"{NpcConfig}\n" +
                   $"{PlayerConfig}";
        }

        public static (bool success, string reason) TryToLoad(MainConfig config)
        {
            config.PlayerConfig = new PlayerConfig();
            config.WoundConfig = new WoundConfig();
            config.NpcConfig = new NpcConfig();

            string path = null;

            if (File.Exists(GswConfigPath))
            {
                path = GswConfigPath;
            }
            else if (File.Exists(ScriptConfigPath))
            {
                path = ScriptConfigPath;
            }

            if (string.IsNullOrEmpty(path))
            {
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
            try
            {
                section = nameof(HotkeysSection);
                HotkeysSection(config, doc);

                section = nameof(PlayerSection);
                PlayerSection(config, doc);

                section = nameof(PedsSection);
                PedsSection(config, doc);

                section = nameof(NotificationsSection);
                NotificationsSection(config, doc);

                section = nameof(WoundsSection);
                WoundsSection(config, doc);

                section = nameof(WeaponsSection);
                WeaponsSection(config, doc);
            }
            catch (Exception e)
            {
                return (false, $"Failed loading of {section}:\n{e.Message}");
            }

            return (true, null);
        }

        private static void HotkeysSection(MainConfig config, XElement doc)
        {
            var node = doc.Element("Hotkeys");
            if (node == null) return;

            config.HelmetKey = node.GetKey("GetHelmetKey");
            config.CheckKey = node.GetKey("CheckKey");
            config.HealKey = node.GetKey("HealKey");
            config.IncreaseRangeKey = node.GetKey("IncreaseRangeKey");
            config.ReduceRangeKey = node.GetKey("ReduceRangeKey");
            config.BandageKey = node.GetKey("BandageKey");
            config.PauseKey = node.GetKey("PauseKey");
        }

        private static void PlayerSection(MainConfig config, XElement doc)
        {
            var node = doc.Element("Player");
            if (node == null) return;

            config.PlayerConfig.WoundedPlayerEnabled = node.Element("GSWPlayerEnabled").GetBool();
            config.PlayerConfig.MaximalPain = node.Element("MaximalPain").GetFloat();
            config.PlayerConfig.PainRecoverSpeed = node.Element("PainRecoverySpeed").GetFloat();
            config.PlayerConfig.BleedHealingSpeed = node.Element("BleedHealSpeed").GetFloat() / 1000f;
            config.PlayerConfig.PoliceCanForgetYou = node.Element("PoliceCanForget").GetBool();
            config.PlayerConfig.CanDropWeapon = node.Element("CanDropWeapon").GetBool();
            config.PlayerConfig.BlipsToMedkits = node.Element("BlipsToMedkits").GetBool();
            config.PlayerConfig.TimeToRefreshMedkits = node.Element("BlipsToMedkits").GetFloat("RefreshTime");
            config.PlayerConfig.MedkitModel = node.Element("BlipsToMedkits").GetString("ModelName");
            config.PlayerConfig.MaximalSlowMo = node.Element("MaximalSlowMo").GetFloat();
            config.PlayerConfig.MoneyForHelmet = node.Element("HelmetCost").GetInt();

            var animationNode = node.Element("MoveSets");
            if (animationNode == null) return;

            config.PlayerConfig.MildPainSets = animationNode.Attribute("MildPain")?.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            config.PlayerConfig.AvgPainSets = animationNode.Attribute("AvgPain")?.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            config.PlayerConfig.IntensePainSets = animationNode.Attribute("IntensePain")?.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void PedsSection(MainConfig config, XElement doc)
        {
            var node = doc.Element("Peds");
            if (node == null) return;

            config.NpcConfig.AddingPedRange = node.Element("GSWScanRange").GetFloat();
            config.NpcConfig.RemovePedRange = config.NpcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

            config.NpcConfig.ShowEnemyCriticalMessages = node.Element("CriticalMessages").GetBool();
            config.NpcConfig.ScanOnlyDamaged = node.Element("ScanOnlyDamaged").GetBool();

            var healthNode = node.Element("CustomHealth");
            config.NpcConfig.MinStartHealth = healthNode.GetInt("Min");
            config.NpcConfig.MaxStartHealth = healthNode.GetInt("Max");

            var painNode = node.Element("MaximalPain");
            config.NpcConfig.LowerMaximalPain = painNode.GetFloat("Min");
            config.NpcConfig.UpperMaximalPain = painNode.GetFloat("Max");

            var accuracyNode = node.Element("Accuracy");
            config.NpcConfig.MinAccuracy = accuracyNode.GetInt("Min");
            config.NpcConfig.MaxAccuracy = accuracyNode.GetInt("Max");

            var rateNode = node.Element("ShootRate");
            config.NpcConfig.MinShootRate = rateNode.GetInt("Min");
            config.NpcConfig.MaxShootRate = rateNode.GetInt("Max");

            config.NpcConfig.MaximalPainRecoverSpeed = node.Element("PainRecoverySpeed").GetFloat();
            config.NpcConfig.MaximalBleedStopSpeed = node.Element("BleedHealSpeed").GetFloat() / 1000f;

            var targetsNode = node.Element("Targets");
            var all = targetsNode.GetBool("ALL");
            GswTargets targets = 0;
            if (all)
            {
                config.NpcConfig.Targets = GswTargets.ALL;
                return;
            }

            if (targetsNode.GetBool("COMPANION"))
            {
                targets |= GswTargets.COMPANION;
            }

            if (targetsNode.GetBool("DISLIKE"))
            {
                targets |= GswTargets.DISLIKE;
            }

            if (targetsNode.GetBool("HATE"))
            {
                targets |= GswTargets.HATE;
            }

            if (targetsNode.GetBool("LIKE"))
            {
                targets |= GswTargets.LIKE;
            }

            if (targetsNode.GetBool("NEUTRAL"))
            {
                targets |= GswTargets.NEUTRAL;
            }

            if (targetsNode.GetBool("PEDESTRIAN"))
            {
                targets |= GswTargets.PEDESTRIAN;
            }

            if (targetsNode.GetBool("RESPECT"))
            {
                targets |= GswTargets.RESPECT;
            }

            config.NpcConfig.Targets = targets;

            var animationNode = node.Element("MoveSets");
            if (animationNode == null) return;

            config.NpcConfig.MildPainSets = animationNode.Attribute("MildPain")?.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            config.NpcConfig.AvgPainSets = animationNode.Attribute("AvgPain")?.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            config.NpcConfig.IntensePainSets = animationNode.Attribute("IntensePain")?.Value.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void NotificationsSection(MainConfig config, XElement doc)
        {
            var node = doc.Element("Notifications");
            if (node == null) return;

            config.Language = node.Element("Language").Attribute("Value").Value;
            config.CommonMessages = node.Element("Common").GetBool();
            config.WarningMessages = node.Element("Warning").GetBool();
            config.AlertMessages = node.Element("Alert").GetBool();
            config.EmergencyMessages = node.Element("Emergency").GetBool();
        }

        private static void WeaponsSection(MainConfig config, XElement doc)
        {
            var node = doc.Element("Weapons");
            if (node == null) return;

            var dictionary = new Dictionary<string, float?[]>();
            foreach (var element in node.Elements())
            {
                var multipliers = new float?[5];

                var damageString = element.Attribute("DamageMult");
                multipliers[0] = damageString != null
                    ? float.Parse(damageString.Value, CultureInfo.InvariantCulture)
                    : null;

                var bleedingString = element.Attribute("BleedingMult");
                multipliers[1] = bleedingString != null
                    ? float.Parse(bleedingString.Value, CultureInfo.InvariantCulture)
                    : null;

                var painString = element.Attribute("PainMult");
                multipliers[2] = painString != null
                    ? float.Parse(painString.Value, CultureInfo.InvariantCulture)
                    : null;

                var critString = element.Attribute("CritChance");
                multipliers[3] = critString != null
                    ? float.Parse(critString.Value, CultureInfo.InvariantCulture)
                    : null;

                var armorString = element.Attribute("ArmorDamage");
                multipliers[4] = armorString != null
                    ? float.Parse(armorString.Value, CultureInfo.InvariantCulture)
                    : null;

                dictionary.Add(element.Name.LocalName, multipliers);
            }

            config.SmallCaliberHashes = GetWeaponHashes("SmallCaliber");
            config.MediumCaliberHashes = GetWeaponHashes("MediumCaliber");
            config.HeavyCaliberHashes = GetWeaponHashes("HighCaliber");
            config.LightImpactHashes = GetWeaponHashes("LightImpact");
            config.HeavyImpactHashes = GetWeaponHashes("HeavyImpact");
            config.ShotgunHashes = GetWeaponHashes("Shotgun");
            config.CuttingHashes = GetWeaponHashes("Cutting");
            config.IgnoreHashes = GetWeaponHashes("Ignore");
            config.WoundConfig.DamageSystemConfigs = dictionary;

            HashSet<uint> GetWeaponHashes(string weaponName)
            {
                var weaponNode = node.Element(weaponName);
                if (weaponNode == null) throw new Exception($"{weaponName} node not found!");

                const string name = "Hashes";
                var hashes = weaponNode.Element(name)?.Attribute(name)?.Value;
                if (string.IsNullOrEmpty(hashes)) throw new Exception($"{weaponName}'s hashes is empty");

                return hashes.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Select(uint.Parse).ToHashSet();
            }
        }

        private static void WoundsSection(MainConfig config, XElement doc)
        {
            var node = doc.Element("Wounds");
            if (node == null) return;

            config.WoundConfig.MoveRateOnFullPain = node.Element("MoveRateOnFullPain").GetFloat();
            config.WoundConfig.RealisticNervesDamage = node.Element("RealisticNervesDamage").GetBool();
            config.WoundConfig.DamageMultiplier = node.Element("OverallDamageMult").GetFloat();
            config.WoundConfig.DamageDeviation = node.Element("DamageDeviation").GetFloat();
            config.WoundConfig.PainMultiplier = node.Element("OverallPainMult").GetFloat();
            config.WoundConfig.PainDeviation = node.Element("PainDeviation").GetFloat();
            config.WoundConfig.BleedingMultiplier = node.Element("OverallBleedingMult").GetFloat();
            config.WoundConfig.BleedingDeviation = node.Element("BleedingDeviation").GetFloat();
            config.WoundConfig.RagdollOnPainfulWound = node.Element("RagdollOnPainfulWound").GetBool();
            config.WoundConfig.PainfulWoundPercent = node.Element("PainfulWoundPercent").GetFloat();
            config.WoundConfig.MinimalChanceForArmorSave = node.Element("MinimalChanceForArmorSave").GetFloat();
            config.WoundConfig.ApplyBandageTime = node.Element("ApplyBandageTime").GetFloat();
            config.WoundConfig.BandageCost = node.Element("BandageCost").GetInt();
            config.WoundConfig.SelfHealingRate = node.Element("SelfHealingRate").GetFloat();
        }
    }
}