namespace GunshotWound2.Utils {
    using System.Globalization;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public static class XElementExtensions {
        public static string GetString(this XElement node, string attributeName = "Value") {
            return string.IsNullOrEmpty(attributeName) ? node?.Value : node?.Attribute(attributeName)?.Value;
        }

        public static bool GetBool(this XElement node, string attributeName = "Value", bool defaultValue = false) {
            string value = GetString(node, attributeName);
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }

        public static int GetInt(this XElement node, string attributeName = "Value", int defaultValue = 0) {
            string value = GetString(node, attributeName);
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public static float GetFloat(this XElement node, string attributeName = "Value", float defaultValue = 0f) {
            string value = GetString(node, attributeName);
            return float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : defaultValue;
        }

        private static Keys? GetKey(this XElement node, string attributeName) {
            string value = node.GetString(attributeName);
            return string.IsNullOrEmpty(value) ? null : (Keys?)System.Enum.Parse(typeof(Keys), value, ignoreCase: true);
        }

        public static InputListener.Scheme GetKeyScheme(this XElement node) {
            Keys? keyCode = node.GetKey("KeyCode");
            Keys? modifiers = node.GetKey("Modifiers");
            return keyCode.HasValue ? new InputListener.Scheme(keyCode.Value, modifiers ?? default) : default;
        }
    }
}