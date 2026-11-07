namespace GunshotWound2.Utils {
    using System;
    using SHVDN;

    public interface ILogger {
        void WriteInfo(string message);
        void WriteWarning(string message);
        void WriteError(string message);
    }

    public sealed class ScriptHookLogger : ILogger {
        private readonly SHVDN.Console consoleInstance;
        private readonly string[] cachedMessages;

        public ScriptHookLogger() {
            consoleInstance = (SHVDN.Console)AppDomain.CurrentDomain.GetData("Console");
            cachedMessages = new string[1];
        }

        public void WriteInfo(string message) {
#if SHVDN_NIGHTLY
            consoleInstance.Print(Log.Level.Info, message);
#else
            consoleInstance.PrintInfo(message);
#endif
            cachedMessages[0] = message;
            Log.Message(Log.Level.Info, cachedMessages);
        }

        public void WriteWarning(string message) {
#if SHVDN_NIGHTLY
            consoleInstance.Print(Log.Level.Warning, message);
#endif
            cachedMessages[0] = message;
            Log.Message(Log.Level.Warning, cachedMessages);
        }

        public void WriteError(string message) {
#if SHVDN_NIGHTLY
            consoleInstance.Print(Log.Level.Error, message);
#endif
            cachedMessages[0] = message;
            Log.Message(Log.Level.Error, cachedMessages);
        }
    }
}