namespace GunshotWound2.PlayerFeature {
    using System;
    using GTA;
    using GTA.Native;

    public static class PlayerEffects {
        private static readonly Control[] CONTROL_VALUES = (Control[])Enum.GetValues(typeof(Control));
        private static readonly string[] CONTROL_NAMES = Enum.GetNames(typeof(Control));

        private static Player Player => Game.Player;

        public static void SetSpecialAbilityLock(bool lockAbility) {
            Player player = Player;
            if (lockAbility && Function.Call<bool>(Hash.IS_SPECIAL_ABILITY_ACTIVE, player)) {
                Function.Call(Hash.SPECIAL_ABILITY_DEACTIVATE, player);
            }

            Hash function = lockAbility
                    ? Hash.SPECIAL_ABILITY_LOCK
                    : Hash.SPECIAL_ABILITY_UNLOCK;

            int playerModel = player.Character.Model.Hash;
            Function.Call(function, playerModel);
            Function.Call(Hash.FLASH_ABILITY_BAR, 1000);
        }

        public static void SetSprint(bool sprint) {
            Function.Call(Hash.SET_PLAYER_SPRINT, Player, sprint);
        }

        public static float GetStaminaRemaining() {
            return Function.Call<float>(Hash.GET_PLAYER_SPRINT_STAMINA_REMAINING, Player);
        }

        public static void DisableVehicleControlThisFrame() {
            for (var i = 0; i < CONTROL_VALUES.Length; i++) {
                Control value = CONTROL_VALUES[i];
                if (value == Control.VehicleExit) {
                    continue;
                }

                if (CONTROL_NAMES[i].StartsWith("Vehicle")) {
                    Game.DisableControlThisFrame(value);
                }
            }
        }

        public static bool InRampageScenario() {
            return Function.Call<bool>(Hash.IS_SCENARIO_GROUP_ENABLED, "Rampage1");
        }
    }
}