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

        public static float NextFloat(this Random rand, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("min must be less than max");
            }

            return (float) rand.NextDouble() * (max - min) + min;
        }

        public static T Next<T>(this Random random, T[] array) {
            int index = random.Next(0, array.Length);
            return array[index];
        }
    }
}