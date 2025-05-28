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
        public const float MINIMAL_RANGE_FOR_WOUNDED_PEDS = 0;
        public const float ADDING_TO_REMOVING_MULTIPLIER = 2;

        public float AddingPedRange;
        public float RemovePedRange;

        public GswTargets Targets;
        public bool ScanOnlyDamaged;
        public bool InstantDeathHeadshot;
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

        public PainMoveSets MalePainMoveSets;
        public PainMoveSets FemalePainMoveSets;

        public string sectionName => "Peds.xml";

        public void FillFrom(XDocument doc) {
            XElement root = doc.Element(nameof(PedsConfig))!;

            AddingPedRange = root.Element("GSWScanRange").GetFloat();
            RemovePedRange = AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

            ShowEnemyCriticalMessages = root.Element("CriticalMessages").GetBool();
            ScanOnlyDamaged = root.Element(nameof(ScanOnlyDamaged)).GetBool();
            InstantDeathHeadshot = root.Element("HeadshotIsInstantDeath").GetBool();
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

            MalePainMoveSets = PainMoveSets.FromXElement(root, nameof(MalePainMoveSets));
            FemalePainMoveSets = PainMoveSets.FromXElement(root, nameof(FemalePainMoveSets));

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
                   + $"{nameof(ScanOnlyDamaged)}: {ScanOnlyDamaged.ToString()}\n"
                   + $"{nameof(AddingPedRange)}: {AddingPedRange.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(RemovePedRange)}: {RemovePedRange.ToString(CultureInfo.InvariantCulture)}\n"
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