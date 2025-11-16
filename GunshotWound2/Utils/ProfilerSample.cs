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

            double frameTimeInMs = stopwatch.Elapsed.TotalMilliseconds;
            if (!firstFrameAccounted) {
                firstFrameTime = frameTimeInMs;
                firstFrameAccounted = true;
            } else if (!profilerIsReady) {
                minFrameTime = frameTimeInMs;
                avgFrameTime = frameTimeInMs;
                maxFrameTime = frameTimeInMs;
                profilerIsReady = true;
            } else {
                minFrameTime = Math.Min(frameTimeInMs, minFrameTime);
                avgFrameTime = avgFrameTime * (1 - AVG_ALPHA) + frameTimeInMs * AVG_ALPHA;
                if (frameTimeInMs >= maxFrameTime) {
                    maxFrameTime = frameTimeInMs;
                } else {
                    maxFrameTime += (frameTimeInMs - maxFrameTime) * (1 - MAX_DECAY);
                }

                double isBadFrame = frameTimeInMs > avgFrameTime * 2.0 ? 1.0 : 0.0;
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