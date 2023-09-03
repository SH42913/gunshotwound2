﻿namespace GunshotWound2 {
    using System.Diagnostics;
    using Configs;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class SharedData {
        public readonly ILogger logger;
        public readonly Stopwatch stopwatch;
        public readonly System.Random random;
        public readonly Weighted_Randomizer.IWeightedRandomizer<int> weightRandom;
        public readonly Notifier notifier;

        public readonly WorldService worldService;
        public readonly MainConfig mainConfig;
        public readonly LocaleConfig localeConfig;

        public Entity playerEntity;
        public float deltaTime;

        public SharedData(ILogger logger) {
            this.logger = logger;
            stopwatch = new Stopwatch();
            random = new System.Random();
            weightRandom = new Weighted_Randomizer.StaticWeightedRandomizer<int>();
            notifier = new Notifier();

            worldService = new WorldService(startCapacity: 64);
            mainConfig = new MainConfig();
            localeConfig = new LocaleConfig();

            playerEntity = null;
        }

        public bool TryGetPlayer(out Entity player) {
            player = playerEntity;
            return !player.IsNullOrDisposed();
        }
    }
}