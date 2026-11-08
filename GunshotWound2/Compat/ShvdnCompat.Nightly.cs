namespace GunshotWound2.Compat {
    using GTA;
    using GTA.Input;
    using GTA.Math;
    using SHVDN;

    /// <summary>SHVDN Nightly API bindings (single variant compiled per build).</summary>
    public static class ShvdnCompat {
        private static readonly ControlAction[] CONTROL_VALUES = (ControlAction[])System.Enum.GetValues(typeof(ControlAction));
        private static readonly string[] CONTROL_NAMES = System.Enum.GetNames(typeof(ControlAction));

        public static float VectorDistance(Vector3 from, Vector3 to) {
            return Vector3.Distance(from, to);
        }

        public static Ped CreateRandomPed(Vector3 position) {
            return Ped.CreateRandom(position);
        }

        public static void ConsolePrintInfo(Console console, string message) {
            console.Print(Log.Level.Info, message);
        }

        public static void ConsolePrintWarning(Console console, string message) {
            console.Print(Log.Level.Warning, message);
        }

        public static void ConsolePrintError(Console console, string message) {
            console.Print(Log.Level.Error, message);
        }

        public static void DisableVehicleControlsThisFrame() {
            for (var i = 0; i < CONTROL_VALUES.Length; i++) {
                if (CONTROL_NAMES[i].StartsWith("Vehicle")) {
                    Controls.DisableControlActionThisFrame(ControlType.PlayerControl, CONTROL_VALUES[i], affectRelatedActions: true);
                }
            }
        }
    }
}