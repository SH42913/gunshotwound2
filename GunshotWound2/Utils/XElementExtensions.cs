namespace GunshotWound2.Utils {
    using System.Globalization;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public static class XElementExtensions {
        public static string GetString(this XElement node, string attributeName = "Value") {
            return string.IsNullOrEmpty(attributeName) ? node?.Value : node?.Attribute(attributeName)?.Value;
        }

        public static bool GetBool(this XElement node, string attributeName = "Value") {
            string value = GetString(node, attributeName);
            return !string.IsNullOrEmpty(value) && bool.Parse(value);
        }

        public static int GetInt(this XElement node, string attributeName = "Value") {
            string value = GetString(node, attributeName);
            return int.Parse(value);
        }

        public static float GetFloat(this XElement node, string attributeName = "Value") {
            string value = GetString(node, attributeName);
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public static Keys? GetKey(this XElement node, string attributeName) {
            string value = node.GetString(attributeName);
            return string.IsNullOrEmpty(value) ? null : (Keys)int.Parse(value);
        }

        public static InputListener.Scheme GetKeyScheme(this XElement node) {
            Keys? keyCode = node.GetKey("KeyCode");
            Keys? modifiers = node.GetKey("Modifiers");
            return keyCode.HasValue ? new InputListener.Scheme(keyCode.Value, modifiers ?? default) : default;
        }
    }
}