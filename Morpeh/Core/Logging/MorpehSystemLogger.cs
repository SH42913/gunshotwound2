namespace Scellecs.Morpeh.Logging {
    using System;

    internal class MorpehSystemLogger : IMorpehLogger {
        void IMorpehLogger.Log(string message) => Console.WriteLine(message);
        void IMorpehLogger.LogWarning(string message) => Console.WriteLine(message);
        void IMorpehLogger.LogError(string message) => Console.WriteLine(message);

        void IMorpehLogger.LogException(Exception exception) => Console.WriteLine(exception);

        //todo
        void IMorpehLogger.BeginSample(string name) { }

        //todo
        void IMorpehLogger.EndSample() { }
    }
}