namespace GunshotWound2 {
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using Configs;
    using GTA;
    using Scellecs.Morpeh;
    using Utils;
    using EcsWorld = Scellecs.Morpeh.World;

    // ReSharper disable once UnusedType.Global
    public sealed class GunshotWound2 : Script {
        private static readonly string EXCEPTION_LOG_PATH = Path.Combine(Application.StartupPath, "GSW2Exception.log");
        private static int PAUSE_POST;

#if DEBUG

        // ReSharper disable once InconsistentNaming
        public static SharedData sharedData;
#else
        private readonly SharedData sharedData;
#endif

        private readonly EcsWorld ecsWorld;
        private readonly SystemsGroup commonSystems;

        private bool isStarted;
        private bool isPaused;
        private bool cleanedUp;

        public GunshotWound2() {
            sharedData = new SharedData(Filename, new ScriptHookLogger());

            ecsWorld = EcsWorld.Create();
            commonSystems = ecsWorld.CreateSystemsGroup();

            KeyUp += OnKeyUp;
            isPaused = false;

            Tick += OnTick;
            Aborted += Cleanup;

            sharedData.logger.WriteInfo("GSW2 is initializing...");
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
            if (sharedData.mainConfig.PauseKey.IsPressed(eventArgs)) {
                TogglePause();
                return;
            }

            if (isStarted && !isPaused) {
                sharedData.inputListener.ConsumeKeyUp(eventArgs);
            }
        }

        private void TogglePause() {
            isPaused = !isPaused;

            LocaleConfig localeConfig = sharedData.localeConfig;
            PAUSE_POST = isPaused
                    ? sharedData.notifier.ReplaceOne(localeConfig.GswIsPaused, blinking: true, PAUSE_POST, Notifier.Color.YELLOW)
                    : sharedData.notifier.ReplaceOne(localeConfig.GswIsWorking, blinking: true, PAUSE_POST, Notifier.Color.GREEN);
        }

        private void Cleanup(object sender, EventArgs e) {
            if (cleanedUp) {
                return;
            }

            try {
                commonSystems.Dispose();
                ecsWorld.Dispose();
                cleanedUp = true;
            } catch (Exception exception) {
                HandleRuntimeException(exception);
            }
        }

        #region PREPARE
        private bool IsStarted() {
            if (isStarted) {
                return true;
            }

            if (Game.IsLoading || Game.IsCutsceneActive) {
                return false;
            }

            (bool success, string reason) = MainConfig.TryToLoad(sharedData.scriptPath, sharedData.mainConfig);
            if (!success) {
                var message = $"GSW2 couldn't load config!\nReason:\n~r~{reason}";
                sharedData.notifier.ShowOne(message, blinking: true);
                sharedData.logger.WriteError(message);
                Abort();
                return false;
            }

            (success, reason) = LocaleConfig.TryToLoad(sharedData.scriptPath, sharedData.localeConfig, sharedData.mainConfig.Language);
            if (!success) {
                var message = $"GSW2 couldn't load localization!\nReason:\n~r~{reason}";
                sharedData.notifier.ShowOne(message, blinking: true);
                sharedData.logger.WriteError(message);
                Abort();
                return false;
            }

            try {
                ClearExceptionLog();
                sharedData.mainConfig.ApplyTo(sharedData.notifier);
                RegisterSystems();
            } catch (Exception e) {
                HandleRuntimeException(e);
                Abort();
                return false;
            }

            var builder = new StringBuilder();
            builder.Append(sharedData.localeConfig.ThanksForUsing);
            builder.AppendEndOfLine();
            builder.AppendLine("~g~GunShot Wound ~r~2~s~\nby <C>SH42913</C>");
            builder.Append($"Translated by {sharedData.localeConfig.LocalizationAuthor ?? "GSW2-community"}");
            sharedData.notifier.ShowOne(builder.ToString(), blinking: true);

#if DEBUG
            sharedData.cheatListener.Register("GSW_TEST", () => {
                foreach (Ped ped in GTA.World.GetAllPeds()) {
                    ped.SetConfigFlag(PedConfigFlagToggles.IsInjured, true);
                }
            });
#endif

            sharedData.logger.WriteInfo($"GunShot Wound 2 ({Application.ProductVersion}) has started");
            isStarted = true;
            return true;
        }

        private void RegisterSystems() {
            PedsFeature.PedsFeature.Create(commonSystems, sharedData);
            PlayerFeature.PlayerFeature.Create(commonSystems, sharedData);
            HitDetection.DetectHitFeature.Create(commonSystems, sharedData);
            WoundFeature.WoundFeature.Create(commonSystems, sharedData);
            HealthFeature.HealthFeature.Create(ecsWorld, commonSystems, sharedData);
            PainFeature.PainFeature.Create(commonSystems, sharedData);
            CritsFeature.CritsFeature.Create(commonSystems, sharedData);
        }
        #endregion

        #region TICK
        private void GunshotWoundTick() {
            if (!isPaused) {
                sharedData.cheatListener.Check();
                commonSystems.Update(sharedData.deltaTime);
                commonSystems.LateUpdate(sharedData.deltaTime);
                commonSystems.CleanupUpdate(sharedData.deltaTime);
            }

            sharedData.notifier.Show();
        }

        private static void ClearExceptionLog() {
            if (File.Exists(EXCEPTION_LOG_PATH)) {
                File.Delete(EXCEPTION_LOG_PATH);
            }
        }

        private static void HandleRuntimeException(Exception exception) {
            sharedData.notifier.ShowOne(sharedData.localeConfig.GswStopped, blinking: true, Notifier.Color.ORANGE);
            sharedData.logger.WriteError(exception.ToString());
            File.WriteAllText(EXCEPTION_LOG_PATH, exception.ToString());
            sharedData.notifier.ShowOne("There is a runtime error in GSW2, please report it to Github-issues!\n"
                                        + $"Log-File - {EXCEPTION_LOG_PATH} to Github-issues\n", blinking: true, Notifier.Color.RED);
        }
        #endregion
    }
}