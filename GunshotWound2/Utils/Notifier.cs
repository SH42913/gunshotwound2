namespace GunshotWound2.Utils {
    using System.Collections.Generic;
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
            info = new Entry("");
            warning = new Entry("~y~");
            alert = new Entry("~o~");
            emergency = new Entry("~r~");
        }

        public void Show() {
            emergency.ShowNotification(stringBuilder);
            alert.ShowNotification(stringBuilder);
            warning.ShowNotification(stringBuilder);
            info.ShowNotification(stringBuilder);
        }

        public sealed class Entry {
            public bool show;

            private readonly string prefix;
            private List<string> messages;

            public Entry(string prefix) {
                this.prefix = prefix;
            }

            public void AddMessage(string message) {
                if (show && !string.IsNullOrWhiteSpace(message)) {
                    messages ??= new List<string>(8);
                    messages.Add(message);
                }
            }

            public void ShowNotification(StringBuilder builder) {
                if (messages == null || messages.Count < 1) {
                    return;
                }

                builder.Clear();
                builder.Append(prefix);
                for (int index = 0, count = messages.Count; index < count; index++) {
                    string message = messages[index];
                    if (count > index + 1) {
                        builder.AppendLine(message);
                    } else {
                        builder.Append(message);
                    }
                }

                messages.Clear();
                Notification.Show(builder.ToString());
            }
        }
    }
}