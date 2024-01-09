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
                Notification.Show(stringBuilder.ToString());
                stringBuilder.Clear();
            }
        }

        public sealed class Entry {
            public bool show;

            private readonly StringBuilder builder;
            private readonly string prefix;

            public Entry(string prefix, StringBuilder builder) {
                this.prefix = prefix;
                this.builder = builder;
            }

            public void AddMessage(string message) {
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