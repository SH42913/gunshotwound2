namespace GunshotWound2.Compat {
    using System;
    using GTA;
    using GTA.Input;
    using GTA.Math;
    using SHVDN;

    /// <summary>SHVDN Nightly API bindings (single variant compiled per build).</summary>
    public static class ShvdnCompat {
        private static readonly ControlAction[] CONTROL_VALUES = (ControlAction[])Enum.GetValues(typeof(ControlAction));
        private static readonly string[] CONTROL_NAMES = Enum.GetNames(typeof(ControlAction));

        public static float VectorDistance(Vector3 from, Vector3 to) {
            return Vector3.Distance(from, to);
        }

        public static Ped CreateRandomPed(Vector3 position) {
            return Ped.CreateRandom(position);
        }

        public static void ConsolePrintInfo(SHVDN.Console console, string message) {
            console.Print(Log.Level.Info, message);
        }

        public static void ConsolePrintWarning(SHVDN.Console console, string message) {
            console.Print(Log.Level.Warning, message);
        }

        public static void ConsolePrintError(SHVDN.Console console, string message) {
            console.Print(Log.Level.Error, message);
        }

        public static void DisableVehicleControlsThisFrame() {
            for (var i = 0; i < CONTROL_VALUES.Length; i++) {
                if (CONTROL_NAMES[i].StartsWith("Vehicle")) {
                    Controls.DisableControlActionThisFrame(ControlType.PlayerControl, CONTROL_VALUES[i], affectRelatedActions: true);
                }
            }
        }

        public static void MergeLatestEntityDamageRecord(Ped ped, ref uint weaponHash, ref int attackerHandle, ref int gameTime) {
            for (uint i = 0; i < 3; i++) {
                var record = NativeMemory.GetEntityDamageRecordEntryAtIndex(ped.MemoryAddress, i);
                if (record.GameTime > gameTime) {
                    weaponHash = unchecked((uint)record.WeaponHash);
                    attackerHandle = record.AttackerEntityHandle;
                    gameTime = record.GameTime;
                }
            }
        }
    }
}