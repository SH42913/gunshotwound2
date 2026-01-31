using System;
using System.Globalization;

// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
    using System.Xml.Linq;
    using Utils;

    [Flags]
    public enum GswTargets {
        ALL = 0,
        COMPANION = 1 << 0,
        DISLIKE = 1 << 1,
        HATE = 1 << 2,
        LIKE = 1 << 3,
        NEUTRAL = 1 << 4,
        PEDESTRIAN = 1 << 5,
        RESPECT = 1 << 6,
    }

    public sealed class PedsConfig : MainConfig.IConfig {
        public GswTargets Targets;
        public bool UseVanillaHealthSystem;
        public bool InstantDeathHeadshot;
        public bool ShowFullHealthInfo;
        public bool DontActivateRagdollFromBulletImpact;
        public bool RealisticSpineDamage;
        public float ClosestPedRange;

        public int MinAccuracy;
        public int MaxAccuracy;

        public int MinShootRate;
        public int MaxShootRate;

        public bool ShowEnemyCriticalMessages;
        public int MinStartHealth;
        public int MaxStartHealth;
        public float MaximalBleedStopSpeed;
        public float SelfHealingRate;

        public float MinPainShockThreshold;
        public float MaxPainShockThreshold;
        public float MaximalPainRecoverSpeed;

        public StatusMoveSets MaleStatusMoveSets;
        public StatusMoveSets FemaleStatusMoveSets;

        public string sectionName => "Peds.xml";
        public bool CustomAccuracyEnabled => MinAccuracy > 0 && MaxAccuracy > 0;
        public bool CustomShootRateEnabled => MinShootRate > 0 && MaxShootRate > 0;

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(PedsConfig))!;

            UseVanillaHealthSystem = root.Element(nameof(UseVanillaHealthSystem)).GetBool();
            ShowEnemyCriticalMessages = root.Element("CriticalMessages").GetBool();
            InstantDeathHeadshot = root.Element("HeadshotIsInstantDeath").GetBool();
            ShowFullHealthInfo = root.Element(nameof(ShowFullHealthInfo)).GetBool();
            DontActivateRagdollFromBulletImpact = root.Element(nameof(DontActivateRagdollFromBulletImpact)).GetBool();
            RealisticSpineDamage = root.Element(nameof(RealisticSpineDamage)).GetBool();
            ClosestPedRange = root.Element(nameof(ClosestPedRange)).GetFloat();

            XElement healthNode = root.Element("CustomHealth");
            MinStartHealth = healthNode.GetInt("Min");
            MaxStartHealth = healthNode.GetInt("Max");

            XElement painNode = root.Element("PainShockThreshold");
            MinPainShockThreshold = painNode.GetFloat("Min");
            MaxPainShockThreshold = painNode.GetFloat("Max");

            XElement accuracyNode = root.Element("CustomAccuracy");
            MinAccuracy = accuracyNode.GetInt("Min");
            MaxAccuracy = accuracyNode.GetInt("Max");

            XElement rateNode = root.Element("CustomShootRate");
            MinShootRate = rateNode.GetInt("Min");
            MaxShootRate = rateNode.GetInt("Max");

            MaximalPainRecoverSpeed = root.Element("PainRecoverySpeed").GetFloat();
            MaximalBleedStopSpeed = root.Element("BleedHealSpeed").GetFloat() / 1000f;
            SelfHealingRate = root.Element(nameof(SelfHealingRate)).GetFloat();

            MaleStatusMoveSets = StatusMoveSets.FromXElement(root, nameof(MaleStatusMoveSets));
            FemaleStatusMoveSets = StatusMoveSets.FromXElement(root, nameof(FemaleStatusMoveSets));

            XElement targetsNode = root.Element("Targets");
            GswTargets targets = 0;
            if (targetsNode.GetBool("COMPANION")) {
                targets |= GswTargets.COMPANION;
            }

            if (targetsNode.GetBool("DISLIKE")) {
                targets |= GswTargets.DISLIKE;
            }

            if (targetsNode.GetBool("HATE")) {
                targets |= GswTargets.HATE;
            }

            if (targetsNode.GetBool("LIKE")) {
                targets |= GswTargets.LIKE;
            }

            if (targetsNode.GetBool("NEUTRAL")) {
                targets |= GswTargets.NEUTRAL;
            }

            if (targetsNode.GetBool("PEDESTRIAN")) {
                targets |= GswTargets.PEDESTRIAN;
            }

            if (targetsNode.GetBool("RESPECT")) {
                targets |= GswTargets.RESPECT;
            }

            Targets = targets;
        }

        public void Validate(MainConfig mainConfig, ILogger logger) { }

        public override string ToString() {
            return $"{nameof(PedsConfig)}:\n"
                   + $"{nameof(ShowEnemyCriticalMessages)}: {ShowEnemyCriticalMessages.ToString()}\n"
                   + $"BleedStop: {MaximalBleedStopSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"StartHealth: {MinStartHealth.ToString()} - {MaxStartHealth.ToString()}\n"
                   + $"MaximalPain: {MinPainShockThreshold.ToString(CultureInfo.InvariantCulture)}-{MaxPainShockThreshold.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"Accuracy: {MinAccuracy.ToString()} - {MaxAccuracy.ToString()}\n"
                   + $"ShootRate: {MinShootRate.ToString()} - {MaxShootRate.ToString()}\n"
                   + $"PainRecoverSpeed: {MaximalPainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}