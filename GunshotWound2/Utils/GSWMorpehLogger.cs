namespace GunshotWound2.Utils {
    using System;
    using Scellecs.Morpeh.Logging;

    public class GSWMorpehLogger : IMorpehLogger {
        private readonly ILogger logger;

        public GSWMorpehLogger(ILogger logger) {
            this.logger = logger;
        }

        public void Log(string message) {
            logger.WriteInfo(message);
        }

        public void LogWarning(string message) {
            logger.WriteWarning(message);
        }

        public void LogError(string message) {
            logger.WriteError(message);
        }

        public void LogException(Exception exception) {
            logger.WriteError(exception.ToString());
        }

        public void BeginSample(string name) { }
        public void EndSample() { }
    }
}