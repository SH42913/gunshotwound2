namespace GunshotWound2 {
    using System;
    using System.IO;
    using System.Windows.Forms;
    using GTA;
    using GTA.UI;
    using Scellecs.Morpeh;
    using EcsWorld = Scellecs.Morpeh.World;

    // ReSharper disable once UnusedType.Global
    public sealed class GunshotWound2 : Script {
        private static readonly string EXCEPTION_LOG_PATH = Path.Combine(Application.StartupPath, "GSW2Exception.log");

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
            if (isStarted && !isPaused) {
                sharedData.inputListener.ConsumeKeyUp(eventArgs.KeyCode);

                // TODO
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
            }
        }

        private void TogglePause() {
            isPaused = !isPaused;
            string message = isPaused
                    ? $"~y~{sharedData.localeConfig.GswIsPaused}"
                    : $"~g~{sharedData.localeConfig.GswIsWorking}";

            sharedData.notifier.info.AddMessage(message);
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
            (bool success, string reason) = Configs.MainConfig.TryToLoad(sharedData.mainConfig);
            if (!success) {
                Notification.Show($"GSW2 couldn't load config!\nReason:\n~r~{reason}");
                Abort();
                return false;
            }

            (success, reason) = Configs.LocaleConfig.TryToLoad(sharedData.localeConfig, sharedData.mainConfig.Language);
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

            sharedData.inputListener.RegisterHotkey(sharedData.mainConfig.PauseKey, TogglePause);
            sharedData.notifier.info.AddMessage(sharedData.localeConfig.ThanksForUsing);
            sharedData.notifier.info.AddMessage("~g~GunShot Wound ~r~2~s~ by SH42913");
            sharedData.notifier.info.AddMessage("Translated by");
            sharedData.notifier.info.AddMessage(sharedData.localeConfig.LocalizationAuthor ?? "GSW2-community");

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
    }
}