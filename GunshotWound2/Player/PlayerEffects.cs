namespace GunshotWound2.Player {
    using GTA;
    using GTA.Native;

    public static class PlayerEffects {
        private static Player Player => Game.Player;

        public static void SetSpecialAbilityLock(bool lockAbility) {
            Player player = Player;
            if (lockAbility && Function.Call<bool>(Hash.IS_SPECIAL_ABILITY_ACTIVE, player)) {
                Function.Call(Hash.SPECIAL_ABILITY_DEACTIVATE_FAST, player);
            }

            Hash function = lockAbility
                    ? Hash.SPECIAL_ABILITY_LOCK
                    : Hash.SPECIAL_ABILITY_UNLOCK;

            int playerModel = player.Character.Model.Hash;
            Function.Call(function, playerModel);
        }

        public static void SetSprint(bool sprint) {
            Function.Call(Hash.SET_PLAYER_SPRINT, Player, sprint);
        }
    }
}