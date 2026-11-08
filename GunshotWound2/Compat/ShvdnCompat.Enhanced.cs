namespace GunshotWound2.Compat {
    using System;
    using GTA;
    using GTA.Math;

    /// <summary>SHVDN Enhanced API bindings (single variant compiled per build).</summary>
    public static class ShvdnCompat {
        private static readonly Control[] CONTROL_VALUES = (Control[])Enum.GetValues(typeof(Control));
        private static readonly string[] CONTROL_NAMES = Enum.GetNames(typeof(Control));

        public static float VectorDistance(Vector3 from, Vector3 to) {
            return World.GetDistance(from, to);
        }

        public static Ped CreateRandomPed(Vector3 position) {
            return World.CreateRandomPed(position);
        }

        public static void ConsolePrintInfo(SHVDN.Console console, string message) {
            console.PrintInfo(message);
        }

        public static void ConsolePrintWarning(SHVDN.Console console, string message) {
            // Enhanced: on-screen console warnings go through Log.Message only.
        }

        public static void ConsolePrintError(SHVDN.Console console, string message) {
            // Enhanced: on-screen console errors go through Log.Message only.
        }

        public static void DisableVehicleControlsThisFrame() {
            for (var i = 0; i < CONTROL_VALUES.Length; i++) {
                if (CONTROL_NAMES[i].StartsWith("Vehicle")) {
                    Game.DisableControlThisFrame(CONTROL_VALUES[i]);
                }
            }
        }
    }
}