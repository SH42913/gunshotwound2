namespace GunshotWound2.Utils {
    using System.Text;
    using GTA.UI;

    public sealed class Notifier {
        public readonly Entry info;
        public readonly Entry warning;
        public readonly Entry alert;
        public readonly Entry emergency;
        private readonly StringBuilder stringBuilder;

        public Notifier() {
            stringBuilder = new StringBuilder();
            info = new Entry("~s~", stringBuilder);
            warning = new Entry("~y~", stringBuilder);
            alert = new Entry("~o~", stringBuilder);
            emergency = new Entry("~r~", stringBuilder);
        }

        public void Show() {
            if (stringBuilder.Length > 0) {
                ShowOne(stringBuilder.ToString(), blinking: false);
                stringBuilder.Clear();
            }
        }

        public int ShowOne(string message, bool blinking) {
            return Notification.PostTicker(message, blinking).Handle;
        }

        public int ReplaceOne(string message, bool blinking, int oldPost) {
            HideOne(oldPost);
            return ShowOne(message, blinking);
        }

        public void HideOne(int handle) {
            Notification.Hide(handle);
        }

        public sealed class Entry {
            public bool show;

            private readonly StringBuilder builder;
            public readonly string prefix;

            public Entry(string prefix, StringBuilder builder) {
                this.prefix = prefix;
                this.builder = builder;
            }

            public void QueueMessage(string message) {
                if (show && !string.IsNullOrWhiteSpace(message)) {
                    if (builder.Length > 0) {
                        builder.AppendEndOfLine();
                    }

                    builder.Append(prefix);
                    builder.Append(message);
                }
            }
        }
    }
}