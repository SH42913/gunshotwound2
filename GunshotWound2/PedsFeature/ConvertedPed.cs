namespace GunshotWound2.PedsFeature {
    using GTA;
    using GTA.Native;
    using GTA.NaturalMotion;
    using Scellecs.Morpeh;

    public struct ConvertedPed : IComponent {
        public string name;
        public Ped thisPed;
        public bool isMale;
        public bool isPlayer;
        public int lastFrameHealth;
        public int lastFrameArmor;
        public Bone lastDamagedBone;

        public (int time, RagdollType type) ragdollRequest;
        public int[] nmMessages;
        public CustomHelper nmHelper;
        public bool ragdollReset;
        public bool isRagdoll;

        public int defaultAccuracy;
        public bool hasHandsTremor;
        public bool hasBrokenLegs;
        public bool hasSpineDamage;
        public bool isRestrictToDrive;

        public float moveRate;
        public string moveSetRequest;
        public bool resetMoveSet;
        public bool hasCustomMoveSet;
        public int sprintBlockers;

#if DEBUG
        public Blip customBlip;
#endif
    }

    public static class ConvertedPedExtensions {
        public static void RequestRagdoll(this ref ConvertedPed convertedPed, int timeInMs, RagdollType type = RagdollType.Relax) {
            if (convertedPed.ragdollRequest.time >= 0 && !convertedPed.hasSpineDamage) {
                convertedPed.ragdollRequest = (timeInMs, type);
            }
        }

        public static void RequestPermanentRagdoll(this ref ConvertedPed convertedPed, RagdollType type = RagdollType.Relax) {
            convertedPed.RequestRagdoll(-1, type);
        }

        public static void ResetRagdoll(this ref ConvertedPed convertedPed) {
            if (!convertedPed.hasSpineDamage) {
                convertedPed.ragdollReset = true;
            }
        }

        public static bool IsUsingPhone(this in ConvertedPed convertedPed) {
            return Function.Call<bool>(Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, convertedPed.thisPed);
        }

        public static void BlockSprint(this ref ConvertedPed convertedPed) {
            convertedPed.sprintBlockers++;
        }

        public static void UnBlockSprint(this ref ConvertedPed convertedPed) {
            if (convertedPed.sprintBlockers > 0) {
                convertedPed.sprintBlockers--;
            }
        }

        public static void ResetMoveRate(this ref ConvertedPed convertedPed) {
            convertedPed.moveRate = 1f;
        }
    }
}