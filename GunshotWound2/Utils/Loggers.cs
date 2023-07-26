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
            consoleInstance = (SHVDN.Console) AppDomain.CurrentDomain.GetData("Console");
            cachedMessages = new string[1];
        }

        public void WriteInfo(string message) {
            consoleInstance.PrintInfo(message);
            cachedMessages[0] = message;
            Log.Message(Log.Level.Info, cachedMessages);
        }

        public void WriteWarning(string message) {
            consoleInstance.PrintWarning(message);
            cachedMessages[0] = message;
            Log.Message(Log.Level.Warning, cachedMessages);
        }

        public void WriteError(string message) {
            consoleInstance.PrintError(message);
            cachedMessages[0] = message;
            Log.Message(Log.Level.Error, cachedMessages);
        }
    }
}