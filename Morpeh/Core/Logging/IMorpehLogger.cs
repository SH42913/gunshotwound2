namespace Scellecs.Morpeh.Logging {
    using System;

    public interface IMorpehLogger {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception exception);
        void BeginSample(string name);
        void EndSample();
    }
}