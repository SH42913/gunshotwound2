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

    public sealed class PedsConfig {
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

        public int TakedownRagdollDurationMs;
        public float TakedownPainMult;

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

        public void FillFrom(XDocument doc) {
            XElement node = doc.Element("Peds");
            if (node == null) {
                return;
            }

            AddingPedRange = node.Element("GSWScanRange").GetFloat();
            RemovePedRange = AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

            ShowEnemyCriticalMessages = node.Element("CriticalMessages").GetBool();
            ScanOnlyDamaged = node.Element("ScanOnlyDamaged").GetBool();
            InstantDeathHeadshot = node.Element("HeadshotIsInstantDeath").GetBool();
            DontActivateRagdollFromBulletImpact = node.Element("DontActivateRagdollFromBulletImpact").GetBool();
            RealisticSpineDamage = node.Element("RealisticSpineDamage").GetBool();
            ClosestPedRange = node.Element("ClosestPedRange").GetFloat();

            TakedownRagdollDurationMs = node.Element("TakedownRagdollDurationMs").GetInt();
            TakedownPainMult = node.Element("TakedownPainMult").GetFloat();

            XElement healthNode = node.Element("CustomHealth");
            MinStartHealth = healthNode.GetInt("Min");
            MaxStartHealth = healthNode.GetInt("Max");

            XElement painNode = node.Element("PainShockThreshold");
            MinPainShockThreshold = painNode.GetFloat("Min");
            MaxPainShockThreshold = painNode.GetFloat("Max");

            XElement accuracyNode = node.Element("CustomAccuracy");
            MinAccuracy = accuracyNode.GetInt("Min");
            MaxAccuracy = accuracyNode.GetInt("Max");

            XElement rateNode = node.Element("CustomShootRate");
            MinShootRate = rateNode.GetInt("Min");
            MaxShootRate = rateNode.GetInt("Max");

            MaximalPainRecoverSpeed = node.Element("PainRecoverySpeed").GetFloat();
            MaximalBleedStopSpeed = node.Element("BleedHealSpeed").GetFloat() / 1000f;
            SelfHealingRate = node.Element("SelfHealingRate").GetFloat();

            MalePainMoveSets = PainMoveSets.FromXElement(node, "MaleMoveSets");
            FemalePainMoveSets = PainMoveSets.FromXElement(node, "FemaleMoveSets");

            XElement targetsNode = node.Element("Targets");
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