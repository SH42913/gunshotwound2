namespace GunshotWound2 {
    using System;
    using System.IO;
    using System.Windows.Forms;
    using Configs;
    using GTA;
    using GTA.UI;
    using Scellecs.Morpeh;
    using EcsWorld = Scellecs.Morpeh.World;

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class GunshotWound2 : Script {
        public const float MINIMAL_RANGE_FOR_WOUNDED_PEDS = 0;
        public const float ADDING_TO_REMOVING_MULTIPLIER = 2;

        private static readonly string EXCEPTION_LOG_PATH = $"{Application.StartupPath}/GSW2Exception.log";

        private readonly SharedData sharedData;

        private readonly EcsWorld ecsWorld;
        private readonly SystemsGroup commonSystems;

        private float timeToStart;
        private bool isStarted;
        private bool isPaused;

        public GunshotWound2() {
            Tick += OnTick;

            var logger = new Utils.ScriptHookLogger();
            sharedData = new SharedData(logger);

            ecsWorld = EcsWorld.Create();
            commonSystems = ecsWorld.CreateSystemsGroup();

            KeyUp += OnKeyUp;
            isPaused = false;
            timeToStart = 5f;

            Aborted += Cleanup;
        }

        private void OnTick(object sender, EventArgs eventArgs) {
            sharedData.deltaTime = Game.LastFrameTime;
            if (!IsStarted()) {
                return;
            }

            try {
                GunshotWoundTick();
            } catch (Exception exception) {
                HandleRuntimeException(exception);
                Abort();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs eventArgs) {
            if (isStarted) {
                ProcessKeyCode(eventArgs.KeyCode);
            }
        }

        private void Cleanup(object sender, EventArgs e) {
            commonSystems.Dispose();
            ecsWorld.Dispose();
        }

        #region PREPARE
        private bool IsStarted() {
            if (isStarted) {
                return true;
            }

            if (timeToStart > 0) {
                timeToStart -= sharedData.deltaTime;
                return false;
            }

            //TODO: Coroutine for frames?
            (bool success, string reason) = MainConfig.TryToLoad(sharedData.mainConfig);
            if (!success) {
                Notification.Show($"GSW2 couldn't load config!\nReason:\n~r~{reason}");
                Abort();
                return false;
            }

            (success, reason) = LocaleConfig.TryToLoad(sharedData.localeConfig, sharedData.mainConfig.Language);
            if (!success) {
                Notification.Show("GSW2 couldn't load localization, default localization was loaded.\n"
                                  + $"You need to check or change localization\nReason:\n~r~{reason}");

                return false;
            }

            try {
                sharedData.mainConfig.ApplyTo(sharedData.notifier);
                RegisterSystems();
            } catch (Exception e) {
                HandleRuntimeException(e);
                Abort();
                return false;
            }

            LocaleConfig localeConfig = sharedData.localeConfig;
            string translationAuthor = localeConfig.LocalizationAuthor ?? "GSW2-community";
            Notification.Show($"{localeConfig.ThanksForUsing}\n~g~GunShot Wound ~r~2~s~\n"
                              + $"by SH42913\nTranslated by {translationAuthor}");

            isStarted = true;
            return true;
        }

        private void RegisterSystems() {
            Player.PlayerFeature.CreateSystems(commonSystems, sharedData);
            Peds.ConvertPedsFeature.CreateSystems(commonSystems, sharedData);
            HitDetection.DetectHitFeature.CreateSystems(commonSystems, sharedData);
            Damage.DamageFeature.CreateSystems(commonSystems, sharedData);
            HealthCare.HealthFeature.CreateSystems(commonSystems, sharedData);

            // PlayerConfig playerConfig = sharedData.mainConfig.PlayerConfig;
            // if (playerConfig.WoundedPlayerEnabled) {
            //     everyFrameSystems.Add(new PlayerSystem()).Add(new SpecialAbilityLockSystem());
            //
            //     if (playerConfig.MaximalSlowMo < 1f) {
            //         everyFrameSystems.Add(new AdrenalineSystem());
            //     }
            // }
            //
            // everyFrameSystems.Add(new InstantHealSystem()).Add(new HelmetRequestSystem()).Add(new RagdollSystem())
            //                  .Add(new MoveSetSwitchSystem()).Add(new DebugInfoSystem()).Add(new CameraShakeSystem())
            //                  .Add(new FlashSystem()).Add(new PainRecoverySystem()).Add(new BleedingSystem())
            //                  .Add(new BandageSystem()).Add(new SelfHealingSystem());
            //
            // commonSystems.Add(new ArmorSystem()).Add(new HitDetectSystem()).Add(new BaseWeaponHitSystem())
            //              .Add(new BodyHitSystem()).Add(new HitCleanSystem()).Add(new SmallCaliberDamageSystem())
            //              .Add(new ShotgunDamageSystem()).Add(new LightImpactDamageSystem())
            //              .Add(new HeavyImpactDamageSystem()).Add(new MediumCaliberDamageSystem())
            //              .Add(new HighCaliberDamageSystem()).Add(new CuttingDamageSystem())
            //              .Add(new ExplosionDamageSystem()).Add(new WoundSystem()).Add(new HeartCriticalSystem())
            //              .Add(new LungsCriticalSystem()).Add(new NervesCriticalSystem()).Add(new ArmsCriticalSystem())
            //              .Add(new LegsCriticalSystem()).Add(new GutsCriticalSystem()).Add(new StomachCriticalSystem())
            //              .Add(new IncreasePainSystem()).Add(new NoPainStateSystem()).Add(new MildPainStateSystem())
            //              .Add(new AveragePainStateSystem()).Add(new IntensePainStateSystem())
            //              .Add(new UnbearablePainStateSystem()).Add(new DeadlyPainStateSystem()).Add(new CheckSystem())
            //              .Add(new NotificationSystem());
        }
        #endregion

        #region TICK
        private void GunshotWoundTick() {
            if (!isPaused) {
                // if (sharedData.mainConfig.PlayerConfig.WoundedPlayerEnabled) {
                //     Function.Call(Hash.SET_PLAYER_HEALTH_RECHARGE_MULTIPLIER, Game.Player, 0f);
                //     Function.Call(Hash.SET_AI_WEAPON_DAMAGE_MODIFIER, 0.01f);
                //     Function.Call(Hash.SET_AI_MELEE_WEAPON_DAMAGE_MODIFIER, 0.01f);
                // }

                commonSystems.Update(sharedData.deltaTime);
                commonSystems.LateUpdate(sharedData.deltaTime);
                commonSystems.CleanupUpdate(sharedData.deltaTime);
            }

            sharedData.notifier.Show();
        }

        private void HandleRuntimeException(Exception exception) {
            Notification.Show($"~o~{sharedData.localeConfig.GswStopped}");
            sharedData.logger.WriteError(exception.ToString());
            File.WriteAllText(EXCEPTION_LOG_PATH, exception.ToString());
            Notification.Show($"~r~There is a runtime error in GSW2!\nCheck {EXCEPTION_LOG_PATH}");
        }
        #endregion

        #region KEYS
        private void ProcessKeyCode(Keys keyCode) {
            MainConfig mainConfig = sharedData.mainConfig;
            LocaleConfig localeConfig = sharedData.localeConfig;

            // if (keyCode == mainConfig.HelmetKey) {
            //     ecsWorld.NewEntity().Get<AddHelmetToPlayerEvent>();
            //     return;
            // }
            //
            // if (keyCode == mainConfig.CheckKey) {
            //     CheckPlayer();
            //     return;
            // }
            //
            // if (keyCode == mainConfig.HealKey) {
            //     HealPlayer();
            //     return;
            // }
            //
            // if (keyCode == mainConfig.BandageKey) {
            //     ApplyBandageToPlayer();
            //     return;
            // }

            if (keyCode == mainConfig.IncreaseRangeKey) {
                ChangeRange(5);
                return;
            }

            if (keyCode == mainConfig.ReduceRangeKey) {
                ChangeRange(-5);
                return;
            }

            if (keyCode == mainConfig.PauseKey) {
                isPaused = !isPaused;
                string message = isPaused ? $"~y~{localeConfig.GswIsPaused}" : $"~g~{localeConfig.GswIsWorking}";
                sharedData.notifier.info.AddMessage(message);
            }
        }

        private void ChangeRange(float value) {
            NpcConfig npcConfig = sharedData.mainConfig.NpcConfig;
            if (npcConfig.AddingPedRange + value < MINIMAL_RANGE_FOR_WOUNDED_PEDS) {
                return;
            }

            npcConfig.AddingPedRange += value;
            npcConfig.RemovePedRange = npcConfig.AddingPedRange * ADDING_TO_REMOVING_MULTIPLIER;

            LocaleConfig localeConfig = sharedData.localeConfig;
            sharedData.notifier.info.AddMessage($"{localeConfig.AddingRange}: {npcConfig.AddingPedRange.ToString("F0")}");
            sharedData.notifier.info.AddMessage($"{localeConfig.RemovingRange}: {npcConfig.RemovePedRange.ToString("F0")}");
        }

        // private void CheckPlayer() {
        //     if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
        //         ecsWorld.ScheduleEventWithTarget<ShowHealthStateEvent>(playerEntity);
        //     }
        // }
        //
        // private void HealPlayer() {
        //     if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
        //         ecsWorld.ScheduleEventWithTarget<InstantHealEvent>(playerEntity);
        //     }
        // }
        //
        // private void ApplyBandageToPlayer() {
        //     if (sharedData.TryGetPlayer(out EcsEntity playerEntity)) {
        //         ecsWorld.ScheduleEventWithTarget<ApplyBandageEvent>(playerEntity);
        //     }
        // }
        #endregion
    }
}