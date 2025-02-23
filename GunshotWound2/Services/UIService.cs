namespace GunshotWound2.Services {
    using GTA.UI;
    using Utils;

    // ReSharper disable once InconsistentNaming
    public sealed class UIService {
        private readonly ILogger logger;

        public UIService(ILogger logger) {
            this.logger = logger;
        }

        public void ShowProgressIndicator(string text, bool clockwise = true) {
#if DEBUG
            logger.WriteInfo($"Show progress with text {text}");
#endif
            LoadingSpinnerType type = clockwise ? LoadingSpinnerType.RegularClockwise : LoadingSpinnerType.Clockwise1;
            LoadingPrompt.Show(text, type);
        }

        public void HideProgressIndicator() {
#if DEBUG
            logger.WriteInfo(nameof(HideProgressIndicator));
#endif
            LoadingPrompt.Hide();
        }

        public void ClearAll() {
            HideProgressIndicator();
        }
    }
}