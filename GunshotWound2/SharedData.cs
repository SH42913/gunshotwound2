namespace GunshotWound2 {
    using System;
    using Configs;
    using GTA;
    using HealthFeature;
    using Scellecs.Morpeh;
    using Services;
    using Utils;
    using Weighted_Randomizer;

    public sealed class SharedData {
        public readonly string scriptPath;
        public readonly ILogger logger;
        public readonly Random random;
        public readonly IWeightedRandomizer<int> weightRandom;
        public readonly Notifier notifier;
        public readonly InputListener inputListener;
        public readonly CheatListener cheatListener;
        public readonly CameraService cameraService;
        public readonly UIService uiService;
        public readonly PedStateService pedStateService;
        public readonly ModelCheckerService modelChecker;
        public readonly PauseService pauseService;

        public readonly WorldService worldService;
        public readonly MainConfig mainConfig;
        public readonly LocaleConfig localeConfig;

        public EcsEntity playerEntity;
        public float deltaTime;
        public int deltaTimeInMs;

        public SharedData(string scriptPath, ILogger logger) {
            this.scriptPath = scriptPath;
            this.logger = logger;

            random = new Random();
            weightRandom = new StaticWeightedRandomizer<int>();
            notifier = new Notifier();
            inputListener = new InputListener();
            cheatListener = new CheatListener(this.logger);
            cameraService = new CameraService(this.logger);
            uiService = new UIService(this.logger);

            worldService = new WorldService(startCapacity: 64);
            mainConfig = new MainConfig();
            localeConfig = new LocaleConfig();
            pedStateService = new PedStateService(notifier, mainConfig, localeConfig);
            modelChecker = new ModelCheckerService();
            pauseService = new PauseService(notifier, localeConfig, this.logger);

            playerEntity = null;
        }

        public bool TryGetPlayer(out EcsEntity player) {
            player = playerEntity;
            return !player.IsNullOrDisposed();
        }

        public bool PlayerIsInitialized() {
            if (TryGetPlayer(out EcsEntity entity) && entity.GetComponent<Health>().IsAlive()) {
                return PlayerCanSeeNotification();
            } else {
                return false;
            }
        }

        public bool PlayerCanSeeNotification() {
            if (Game.IsCutsceneActive || !GameplayCamera.IsRendering) {
                return false;
            }

            Player player = Game.Player;
            return player.IsPlaying && player.CanControlCharacter;
        }

        public bool TryGetClosestPedEntity(out Ped closestPed, out EcsEntity closestPedEntity) {
            if (GTAHelpers.TryGetClosestPed(out closestPed, mainConfig.pedsConfig.ClosestPedRange)) {
                return worldService.TryGetConverted(closestPed, out closestPedEntity);
            }

            closestPedEntity = null;
            return false;
        }
    }
}