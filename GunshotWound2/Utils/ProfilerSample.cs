namespace GunshotWound2.Utils {
    using System;

    public sealed class ProfilerSample {
        private const double AVG_ALPHA = 0.1;
        private const double MAX_DECAY = 0.999;
        private const string MS_FORMAT = "F";

        private readonly string name;
        private readonly System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        private double firstFrameTime;
        private double avgFrameTime;
        private double maxFrameTime;
        private double minFrameTime;
        private double badFrameRatio;

        private bool firstFrameAccounted;
        private bool profilerIsReady;

        public ProfilerSample(string name) {
            this.name = name;
        }

        public void Start() {
            stopwatch.Restart();
        }

        public void Stop() {
            stopwatch.Stop();
            long ticks = stopwatch.ElapsedTicks;
            double frameTime = ticks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            if (!firstFrameAccounted) {
                firstFrameTime = frameTime;
                firstFrameAccounted = true;
            } else if (!profilerIsReady) {
                minFrameTime = frameTime;
                avgFrameTime = frameTime;
                maxFrameTime = frameTime;
                profilerIsReady = true;
            } else {
                minFrameTime = Math.Min(frameTime, minFrameTime);
                avgFrameTime = avgFrameTime * (1 - AVG_ALPHA) + frameTime * AVG_ALPHA;
                if (frameTime >= maxFrameTime) {
                    maxFrameTime = frameTime;
                } else {
                    maxFrameTime += (frameTime - maxFrameTime) * (1 - MAX_DECAY);
                }

                double isBadFrame = frameTime > avgFrameTime * 2.0 ? 1.0 : 0.0;
                badFrameRatio = badFrameRatio * (1 - AVG_ALPHA) + isBadFrame * AVG_ALPHA;
            }
        }

        public void OutputCurrentProfilingResults(ILogger logger) {
            logger.WriteInfo($"{name} results:");
            logger.WriteInfo($"First frame:{firstFrameTime.ToString(MS_FORMAT)}ms");
            logger.WriteInfo($"MIN:{minFrameTime.ToString(MS_FORMAT)}ms");
            logger.WriteInfo($"AVG:{avgFrameTime.ToString(MS_FORMAT)}ms");
            logger.WriteInfo($"MAX:{maxFrameTime.ToString(MS_FORMAT)}ms");
            logger.WriteInfo($"BAD:{badFrameRatio.ToString("P")}");
        }
    }
}