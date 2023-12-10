namespace GunshotWound2.Peds {
    using GTA;
    using GTA.Native;
    using Scellecs.Morpeh;

    public struct ConvertedPed : IComponent {
        public string name;
        public Ped thisPed;
        public bool isMale;
        public bool isPlayer;
        public int lastFrameHealth;
        public int lastFrameArmor;
        public (int time, RagdollType type) ragdollRequest;
        public bool ragdollReset;
        public int defaultAccuracy;
#if DEBUG
        public Blip customBlip;
#endif
    }

    public static class ConvertedPedExtensions {
        public static void RequestRagdoll(this ref ConvertedPed convertedPed, int timeInMs, RagdollType type = RagdollType.Relax) {
            convertedPed.ragdollRequest = (timeInMs, type);
        }

        public static void RequestPermanentRagdoll(this ref ConvertedPed convertedPed, RagdollType type = RagdollType.Relax) {
            convertedPed.ragdollRequest = (-1, type);
        }

        public static void ResetRagdoll(this ref ConvertedPed convertedPed) {
            convertedPed.ragdollReset = true;
        }

        public static bool IsUsingPhone(this in ConvertedPed convertedPed) {
            return Function.Call<bool>(Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, convertedPed.thisPed);
        }
    }
}