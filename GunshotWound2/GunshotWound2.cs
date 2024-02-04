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
            sharedData.deltaTimeInMs = (int)(sharedData.deltaTime * 1000f);
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
            if (eventArgs.KeyCode == sharedData.mainConfig.PauseKey) {
                TogglePause();
                return;
            }

            if (isStarted && !isPaused) {
                sharedData.inputListener.ConsumeKeyUp(eventArgs.KeyCode);
            }
        }

        private void TogglePause() {
            isPaused = !isPaused;
            string message = isPaused ? $"~y~{sharedData.localeConfig.GswIsPaused}" : $"~g~{sharedData.localeConfig.GswIsWorking}";

            sharedData.notifier.info.AddMessage(message);
        }

        private void Cleanup(object sender, EventArgs e) {
            try {
                commonSystems.Dispose();
                ecsWorld.Dispose();
            } catch (Exception exception) {
                HandleRuntimeException(exception);
            }
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
                Notification.Show($"GSW2 couldn't load config!\nReason:\n~r~{reason}", blinking: true);
                Abort();
                return false;
            }

            (success, reason) = Configs.LocaleConfig.TryToLoad(sharedData.localeConfig, sharedData.mainConfig.Language);
            if (!success) {
                Notification.Show($"GSW2 couldn't load localization!\nReason:\n~r~{reason}", blinking: true);
                Abort();
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

            sharedData.notifier.info.AddMessage(sharedData.localeConfig.ThanksForUsing);
            sharedData.notifier.info.AddMessage("~g~GunShot Wound ~r~2~s~");
            sharedData.notifier.info.AddMessage("by <C>SH42913</C>");
            sharedData.notifier.info.AddMessage($"\nTranslated by {sharedData.localeConfig.LocalizationAuthor ?? "GSW2-community"}");

            isStarted = true;
            return true;
        }

        private void RegisterSystems() {
            PedsFeature.PedsFeature.Create(commonSystems, sharedData);
            PlayerFeature.PlayerFeature.Create(commonSystems, sharedData);
            HitDetection.DetectHitFeature.Create(commonSystems, sharedData);
            WoundFeature.WoundFeature.Create(commonSystems, sharedData);
            HealthFeature.HealthFeature.Create(ecsWorld, commonSystems, sharedData);
            CritsFeature.CritsFeature.Create(commonSystems, sharedData);
            PainFeature.PainFeature.Create(commonSystems, sharedData);
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