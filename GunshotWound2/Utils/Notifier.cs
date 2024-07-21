namespace GunshotWound2.Utils {
    using System.Text;
    using GTA.UI;

    public sealed class Notifier {
        public readonly Entry info;
        public readonly Entry peds;
        public readonly Entry wounds;
        public readonly Entry critical;

        private readonly StringBuilder stringBuilder;

        public Notifier() {
            stringBuilder = new StringBuilder();
            info = new Entry(Color.COMMON, stringBuilder);
            peds = new Entry(Color.COMMON, stringBuilder);
            wounds = new Entry(Color.ORANGE, stringBuilder);
            critical = new Entry(Color.RED, stringBuilder);
        }

        public void Show() {
            if (stringBuilder.Length > 0) {
                ShowOne(stringBuilder.ToString(), blinking: false);
                stringBuilder.Clear();
            }
        }

        public int ShowOne(string message, bool blinking, Color color = default) {
            color.AppendTo(ref message);
            return Notification.PostTicker(message, blinking).Handle;
        }

        public int ReplaceOne(string message, bool blinking, int oldPost, Color color = default) {
            HideOne(oldPost);
            color.AppendTo(ref message);
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

            public void QueueMessage(string message, Color overrideColor = default) {
                if (show && !string.IsNullOrWhiteSpace(message)) {
                    if (builder.Length > 0) {
                        builder.AppendEndOfLine();
                    }

                    builder.Append(prefix);
                    overrideColor.AppendTo(builder);
                    builder.Append(message);
                }
            }
        }

        public readonly struct Color {
            public static readonly Color COMMON = new("~s~");
            public static readonly Color GREEN = new("~g~");
            public static readonly Color YELLOW = new("~y~");
            public static readonly Color ORANGE = new("~o~");
            public static readonly Color RED = new("~r~");

            public readonly string prefix;

            public bool IsValid => !string.IsNullOrEmpty(prefix);

            private Color(string prefix) {
                this.prefix = prefix;
            }

            public void AppendTo(StringBuilder stringBuilder) {
                if (IsValid) {
                    stringBuilder.Append(prefix);
                }
            }

            public void AppendTo(ref string message) {
                message = prefix + message;
            }

            public static implicit operator string(Color color) {
                return color.prefix;
            }
        }
    }
}