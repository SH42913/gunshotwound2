namespace GunshotWound2.Peds {
    using GTA;
    using GTA.Native;
    using Scellecs.Morpeh;

    public struct ConvertedPed : IComponent {
        public string name;
        public Ped thisPed;
        public bool isPlayer;
        public int lastFrameHealth;
        public int lastFrameArmor;
#if DEBUG
        public Blip customBlip;
#endif
    }

    public static class ConvertedPedExtensions {
        public static bool IsUsingPhone(this in ConvertedPed convertedPed) {
            return Function.Call<bool>(Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, convertedPed.thisPed);
        }
    }
}