namespace GunshotWound2.Utils {
    using System.Text;

    public static class StringBuilderExtensions {
        public static StringBuilder AppendEndOfLine(this StringBuilder stringBuilder) {
            return stringBuilder.Append('\n');
        }

        public static StringBuilder SetDefaultColor(this StringBuilder stringBuilder) {
            return stringBuilder.Append("~s~");
        }
    }
}