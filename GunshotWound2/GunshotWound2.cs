// #define GSW_PROFILING

namespace GunshotWound2 {
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using Configs;
    using GTA;
    using Scellecs.Morpeh;
    using Utils;
    using EcsWorld = Scellecs.Morpeh.World;

    // ReSharper disable once UnusedType.Global
    public sealed class GunshotWound2 : Script {
        private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetCallingAssembly().GetName();
        private static readonly string SCRIPT_NAME = $"{ASSEMBLY_NAME.Name}({ASSEMBLY_NAME.Version})";
        private static readonly string EXCEPTION_LOG_PATH = Path.Combine(Application.StartupPath, "GSW2Exception.log");
        private static int PAUSE_POST;

#if DEBUG

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public static SharedData sharedData;
#else
        private readonly SharedData sharedData;
#endif

        private readonly EcsWorld ecsWorld;
        private readonly SystemsGroup commonSystems;

#if GSW_PROFILING
        private readonly ProfilerSample profilerSample = new("GSW_TICK");
#endif

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

            sharedData.logger.WriteInfo($"{SCRIPT_NAME} is initializing...");
#if DEBUG
            MLogger.SetInstance(new GSWMorpehLogger(sharedData.logger));
#endif
        }

        private void OnTick(object sender, EventArgs eventArgs) {
            if (Game.IsPaused) {
                return;
            }

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
            if (Game.IsPaused) {
                return;
            }

            if (sharedData.mainConfig.PauseKey.IsPressed(eventArgs)) {
                TogglePause();
                return;
            }

            if (isStarted && !isPaused && Game.Player.CanControlCharacter) {
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
#if GSW_PROFILING
                profilerSample.OutputCurrentProfilingResults(sharedData.logger);
#endif
                commonSystems.Dispose();
                ecsWorld.Dispose();
                sharedData.cameraService.ClearAllEffects();
                sharedData.uiService.ClearAll();
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

            if (!sharedData.PlayerCanSeeNotification()) {
                return false;
            }

            sharedData.logger.WriteInfo("GSW2 is loading configs...");
            (bool success, string reason, string trace) = sharedData.mainConfig.TryToLoad(sharedData.scriptPath);
            if (!success) {
                var message = $"GSW2 couldn't load config!\nReason:\n~r~{reason}";
                sharedData.notifier.ShowOne(message, blinking: true);
                sharedData.logger.WriteError(message);
                sharedData.logger.WriteInfo(trace);
                Abort();
                return false;
            }

            sharedData.logger.WriteInfo("GSW2 is validating configs...");
            sharedData.mainConfig.ValidateConfigs(sharedData.logger);

            sharedData.logger.WriteInfo("GSW2 is loading localization...");
            (success, reason) = sharedData.localeConfig.TryToLoad(sharedData.scriptPath, sharedData.mainConfig.Language);
            if (!success) {
                var message = $"GSW2 couldn't load localization!\nReason:\n~r~{reason}";
                sharedData.notifier.ShowOne(message, blinking: true);
                sharedData.logger.WriteError(message);
                Abort();
                return false;
            }

            try {
                sharedData.logger.WriteInfo("GSW2 is starting...");
                ClearExceptionLog();
                WhenConfigLoadedActions();
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
            builder.Append($"Translated by {sharedData.localeConfig.TranslationAuthor ?? "GSW2-community"}");
            sharedData.notifier.ShowOne(builder.ToString(), blinking: true);

#if GSW_PROFILING
            sharedData.cheatListener.Register("GSW_PROFILING_RESULTS", () => {
                profilerSample.OutputCurrentProfilingResults(sharedData.logger);
            });
#endif
#if DEBUG
            sharedData.cheatListener.Register("TEST", () => {
                //
            });
#endif

            sharedData.logger.WriteInfo("GSW2 has started");
            isStarted = true;
            return true;
        }

        private void WhenConfigLoadedActions() {
            sharedData.cameraService.useScreenEffects = sharedData.mainConfig.playerConfig.UseScreenEffects;
            sharedData.modelChecker.Init(sharedData.mainConfig);
            sharedData.mainConfig.ApplyTo(sharedData.notifier);
        }

        private void RegisterSystems() {
            PedsFeature.PedsFeature.Create(commonSystems, sharedData);
            PlayerFeature.PlayerFeature.Create(commonSystems, sharedData);
            HitDetection.DetectHitFeature.Create(commonSystems, sharedData);
            WoundFeature.WoundFeature.Create(commonSystems, sharedData);
            TraumaFeature.TraumaFeature.Create(commonSystems, sharedData);
            HealthFeature.HealthFeature.Create(ecsWorld, commonSystems, sharedData);
            PainFeature.PainFeature.Create(commonSystems, sharedData);
            StatusFeature.StatusFeature.Create(commonSystems, sharedData);
            InventoryFeature.InventoryFeature.Create(commonSystems, sharedData);
        }
        #endregion

        #region TICK
        private void GunshotWoundTick() {
            if (!isPaused) {
#if GSW_PROFILING
                profilerSample.Start();
#endif
                sharedData.cheatListener.Check();
                commonSystems.Update(sharedData.deltaTime);
                commonSystems.LateUpdate(sharedData.deltaTime);
                commonSystems.CleanupUpdate(sharedData.deltaTime);
#if GSW_PROFILING
                profilerSample.Stop();
#endif
#if DEBUG
                //
#endif
            }

            sharedData.notifier.Show();
        }

        private static void ClearExceptionLog() {
            if (File.Exists(EXCEPTION_LOG_PATH)) {
                File.Delete(EXCEPTION_LOG_PATH);
            }
        }

        private void HandleRuntimeException(Exception exception) {
            var log = $"Exception in {SCRIPT_NAME}:\n{exception}";
            File.WriteAllText(EXCEPTION_LOG_PATH, log);
            sharedData.logger.WriteError(log);
            sharedData.notifier.ShowOne(sharedData.localeConfig.GswStopped, blinking: true, Notifier.Color.ORANGE);
            sharedData.notifier.ShowOne("There is a runtime error in GSW2, please report it to Github-issues or GSW discord!\n"
                                        + $"Log-File - {EXCEPTION_LOG_PATH} to Github-issues\n", blinking: true, Notifier.Color.RED);
        }
        #endregion
    }
}