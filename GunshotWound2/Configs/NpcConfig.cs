using System;
using System.Globalization;
// ReSharper disable InconsistentNaming

namespace GunshotWound2.Configs {
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

    public sealed class NpcConfig {
        public float AddingPedRange;
        public float RemovePedRange;

        public GswTargets Targets;
        public bool ScanOnlyDamaged;
        public bool InstantDeathHeadshot;
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

        public float LowerMaximalPain;
        public float UpperMaximalPain;
        public float MaximalPainRecoverSpeed;

        public PainMoveSets MalePainMoveSets;
        public PainMoveSets FemalePainMoveSets;

        public override string ToString() {
            return $"{nameof(NpcConfig)}:\n"
                   + $"{nameof(ScanOnlyDamaged)}: {ScanOnlyDamaged.ToString()}\n"
                   + $"{nameof(AddingPedRange)}: {AddingPedRange.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(RemovePedRange)}: {RemovePedRange.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"{nameof(ShowEnemyCriticalMessages)}: {ShowEnemyCriticalMessages.ToString()}\n"
                   + $"BleedStop: {MaximalBleedStopSpeed.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"StartHealth: {MinStartHealth.ToString()} - {MaxStartHealth.ToString()}\n"
                   + $"MaximalPain: {LowerMaximalPain.ToString(CultureInfo.InvariantCulture)} - {UpperMaximalPain.ToString(CultureInfo.InvariantCulture)}\n"
                   + $"Accuracy: {MinAccuracy.ToString()} - {MaxAccuracy.ToString()}\n"
                   + $"ShootRate: {MinShootRate.ToString()} - {MaxShootRate.ToString()}\n"
                   + $"PainRecoverSpeed: {MaximalPainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}