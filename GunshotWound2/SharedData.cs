namespace GunshotWound2 {
    using System.Diagnostics;
    using Configs;
    using HealthFeature;
    using Scellecs.Morpeh;
    using Utils;

    public sealed class SharedData {
        public readonly ILogger logger;
        public readonly Stopwatch stopwatch;
        public readonly System.Random random;
        public readonly Weighted_Randomizer.IWeightedRandomizer<int> weightRandom;
        public readonly Notifier notifier;
        public readonly InputListener inputListener;
        public readonly CheatListener cheatListener;

        public readonly WorldService worldService;
        public readonly MainConfig mainConfig;
        public readonly LocaleConfig localeConfig;

        public Entity playerEntity;
        public float deltaTime;
        public int deltaTimeInMs;

        public SharedData(ILogger logger) {
            this.logger = logger;
            stopwatch = new Stopwatch();
            random = new System.Random();
            weightRandom = new Weighted_Randomizer.StaticWeightedRandomizer<int>();
            notifier = new Notifier();
            inputListener = new InputListener();
            cheatListener = new CheatListener(this.logger);

            worldService = new WorldService(startCapacity: 64);
            mainConfig = new MainConfig();
            localeConfig = new LocaleConfig();

            playerEntity = null;
        }

        public bool TryGetPlayer(out Entity player) {
            player = playerEntity;
            return !player.IsNullOrDisposed();
        }

        public bool PlayerCanSeeNotification() {
            if (TryGetPlayer(out Entity entity) && entity.GetComponent<Health>().IsAlive()) {
                return !GTA.Game.IsLoading
                       && !GTA.Game.IsCutsceneActive
                       && GTA.Game.Player.IsPlaying
                       && GTA.Game.Player.CanControlCharacter;
            } else {
                return false;
            }
        }
    }
}