using System;

namespace GunshotWound2.Utils
{
    public static class RandomExtensions
    {
        public static bool IsTrueWithProbability(this Random random, double probability)
        {
            if (probability >= 1) return true;
            if (probability <= 0) return false;
            return random.NextDouble() < probability;
        }
    }
}