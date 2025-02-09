namespace GunshotWound2.Utils {
    using GTA;
    using GTA.Math;

    // ReSharper disable once InconsistentNaming
    public static class GTAHelpers {
        public static bool TryGetClosestPed(out Ped closestPed, float radius) {
            Ped playerPed = Game.Player.Character;
            Ped[] closestPeds = World.GetNearbyPeds(playerPed, radius);
            if (closestPeds.Length < 1) {
                closestPed = null;
                return false;
            }

            if (closestPeds.Length == 1) {
                closestPed = closestPeds[0];
                return true;
            }

            var closestPedIndex = 0;
            var minDistance = float.MaxValue;
            Vector3 playerPos = playerPed.Position;
            for (int i = 0; i < closestPeds.Length; i++) {
                Ped ped = closestPeds[i];
                float distance = World.GetDistance(playerPos, ped.Position);
                if (distance < minDistance) {
                    closestPedIndex = i;
                    minDistance = distance;
                }
            }

            closestPed = closestPeds[closestPedIndex];
            return true;
        }
    }
}