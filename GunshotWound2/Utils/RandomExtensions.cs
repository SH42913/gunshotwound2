namespace GunshotWound2.Utils {
    using System;
    using System.Collections.Generic;
    using Weighted_Randomizer;

    public static class RandomExtensions {
        public static bool IsTrueWithProbability(this Random random, double probability) {
            switch (probability) {
                case >= 1: return true;
                case <= 0: return false;
                default:   return random.NextDouble() < probability;
            }
        }

        public static float NextFloat(this Random rand, float min, float max) {
            if (min > max) {
                throw new ArgumentException("min must be less than max");
            }

            return (float)rand.NextDouble() * (max - min) + min;
        }

        public static T Next<T>(this Random random, T[] array) {
            int index = random.Next(0, array.Length);
            return array[index];
        }

        public static T NextFromCollection<T>(this Random random, ICollection<T> collection) {
            if (collection == null || collection.Count < 1) {
                return default;
            }

            int randomIndex = random.Next(0, collection.Count);
            var currentIndex = 0;
            foreach (T item in collection) {
                if (randomIndex == currentIndex++) {
                    return item;
                }
            }

            throw new Exception("Must not happen!");
        }

        public static T GetValueWithWeights<T>(this IWeightedRandomizer<int> random, (T value, int weight)[] array) {
            random.Clear();
            for (var i = 0; i < array.Length; i++) {
                random.Add(i, array[i].weight);
            }

            int woundIndex = random.NextWithRemoval();
            return array[woundIndex].value;
        }
    }
}