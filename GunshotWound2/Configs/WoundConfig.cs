using System.Collections.Generic;

namespace GunshotWound2.Configs
{
    public class WoundConfig
    {
        public float DamageMultiplier;
        public float PainMultiplier;
        public float BleedingMultiplier;

        public float DamageDeviation;
        public float PainDeviation;
        public float BleedingDeviation;
        
        public bool RealisticNervesDamage;

        public float EmergencyBleedingLevel;
        
        public float MoveRateOnFullPain;
        public float MoveRateOnNervesDamage;

        public Dictionary<string, float?[]> DamageSystemConfigs;

        public override string ToString()
        {
            return "WoundConfig:" +
                   $"RealisticNervesDamage: {RealisticNervesDamage}\n" +
                   $"D/P/B Mults: {DamageMultiplier}/{PainMultiplier}/{BleedingMultiplier}\n" +
                   $"D/P/B Deviations: {DamageDeviation}/{PainDeviation}/{BleedingDeviation}\n" +
                   $"MoveRateOnFullPain: {MoveRateOnFullPain}\n" +
                   $"MoveRateOnNervesDamage: {MoveRateOnNervesDamage}\n";
        }
    }
}