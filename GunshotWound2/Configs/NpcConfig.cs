using System;
using System.Globalization;
using GTA;

namespace GunshotWound2.Configs
{
    [Flags]
    public enum GswTargets
    {
        COMPANION = 1,
        DISLIKE = 2,
        HATE = 4,
        LIKE = 8,
        NEUTRAL = 16,
        PEDESTRIAN = 32,
        RESPECT = 64,
        ALL = 128
    }

    public sealed class NpcConfig
    {
        public float AddingPedRange;
        public float RemovePedRange;

        public GswTargets Targets;
        public bool ScanOnlyDamaged;

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

        public string[] MildPainSets;
        public string[] AvgPainSets;
        public string[] IntensePainSets;

        public Ped[] WorldPeds;
        public int LastCheckedPedIndex;
        public int UpperBoundForFindInMs;

        public float GetRandomHealth()
        {
            return GunshotWound2.Random.Next(MinStartHealth, MaxStartHealth);
        }

        public static NpcConfig CreateDefault()
        {
            return new NpcConfig
            {
                AddingPedRange = 50f,
                RemovePedRange = 100f,
                ShowEnemyCriticalMessages = true,
                MinStartHealth = 50,
                MaxStartHealth = 100,
                MaximalBleedStopSpeed = 0.001f,
                LowerMaximalPain = 50,
                UpperMaximalPain = 80,
                MaximalPainRecoverSpeed = 1f,
                UpperBoundForFindInMs = 10,
                MinAccuracy = 10,
                MaxAccuracy = 50,
                MinShootRate = 10,
                MaxShootRate = 50,
                Targets = GswTargets.ALL,
                ScanOnlyDamaged = false
            };
        }

        public override string ToString()
        {
            return $"{nameof(NpcConfig)}:\n" +
                   $"{nameof(ScanOnlyDamaged)}: {ScanOnlyDamaged.ToString()}\n" +
                   $"{nameof(AddingPedRange)}: {AddingPedRange.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(RemovePedRange)}: {RemovePedRange.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"{nameof(ShowEnemyCriticalMessages)}: {ShowEnemyCriticalMessages.ToString()}\n" +
                   $"BleedStop: {MaximalBleedStopSpeed.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"StartHealth: {MinStartHealth.ToString()} - {MaxStartHealth.ToString()}\n" +
                   $"MaximalPain: {LowerMaximalPain.ToString(CultureInfo.InvariantCulture)} - {UpperMaximalPain.ToString(CultureInfo.InvariantCulture)}\n" +
                   $"Accuracy: {MinAccuracy.ToString()} - {MaxAccuracy.ToString()}\n" +
                   $"ShootRate: {MinShootRate.ToString()} - {MaxShootRate.ToString()}\n" +
                   $"PainRecoverSpeed: {MaximalPainRecoverSpeed.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}