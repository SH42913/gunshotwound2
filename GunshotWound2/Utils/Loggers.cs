using SHVDN;

namespace GunshotWound2.Utils {
    public interface ILogger {
        void WriteInfo(string message);
        void WriteWarning(string message);
        void WriteError(string message);
    }

    public sealed class ScriptHookLogger : ILogger {
        private readonly Console consoleInstance;
        private readonly string[] cachedMessages;

        public ScriptHookLogger() {
            consoleInstance = System.AppDomain.CurrentDomain.GetData("Console") as Console;
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