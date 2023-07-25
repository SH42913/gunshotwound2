namespace GunshotWound2.Utils {
    using System.Diagnostics;
    using Configs;
    using Scellecs.Morpeh;
    using World;

    public sealed class SharedData {
        public readonly ILogger logger;
        public readonly Stopwatch stopwatch;
        public readonly System.Random random;

        public readonly GswWorld gswWorld;
        public readonly MainConfig mainConfig;
        public readonly LocaleConfig localeConfig;

        public Entity playerEntity;
        public float deltaTime;

        public SharedData(ILogger logger) {
            this.logger = logger;
            stopwatch = new Stopwatch();
            random = new System.Random();

            gswWorld = new GswWorld(startCapacity: 64);
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