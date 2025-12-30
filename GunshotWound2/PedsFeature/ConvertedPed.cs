namespace GunshotWound2.PedsFeature {
    using GTA;
    using GTA.Native;
    using GTA.NaturalMotion;
    using Scellecs.Morpeh;
    using StatusFeature;
    using Utils;

    public struct ConvertedPed : IComponent {
        public delegate void AfterRagdollAction(ref ConvertedPed convertedPed);

        public string name;
        public Ped thisPed;
        public bool isMale;
        public bool isPlayer;

        public int defaultMaxHealth;
        public int lastFrameHealth;
        public int lastFrameArmor;
        public Ped lastAggressor;
        public IPedStatus status;

        public (int time, RagdollType type) ragdollRequest;
        public bool permanentRagdoll;
        public CustomHelper requestedNmHelper;
        public CustomHelper activeNmHelper;
        public bool ragdollReset;
        public bool isRagdoll;
        public AfterRagdollAction afterRagdollAction;

        public int defaultAccuracy;
        public bool hasHandsTremor;
        public bool hasBrokenLegs;
        public bool hasSpineDamage;
        public bool isRestrictToDrive;
        public bool forceRemove;

        // public (string, string) forcedAnimation;
        public float moveRate;
        public string moveSetRequest;
        public bool resetMoveSet;
        public bool hasCustomMoveSet;
        public int sprintBlockers;

        public string facialIdleAnim;
        public bool facialIdleAnimLock;
        public bool facialIdleAnimApplied;

#if DEBUG
        public Blip customBlip;
#endif
    }

    public static class ConvertedPedExtensions {
        public static float CalculateTimeToDeath(this in ConvertedPed convertedPed, float damagePerSecond) {
            return convertedPed.TotalHealth() / damagePerSecond;
        }

        public static int TotalHealth(this in ConvertedPed convertedPed) {
            return Configs.WoundConfig.ConvertHealthFromNative(convertedPed.thisPed.Health);
        }

        public static void RequestRagdoll(this ref ConvertedPed convertedPed, int timeInMs, RagdollType type = RagdollType.Relax) {
            if (convertedPed.ragdollRequest.time >= 0 && !convertedPed.permanentRagdoll) {
                convertedPed.ragdollRequest = (timeInMs, type);
            }
        }

        public static void RequestPermanentRagdoll(this ref ConvertedPed convertedPed) {
            convertedPed.permanentRagdoll = true;
        }

        public static void RemoveRagdollRequest(this ref ConvertedPed convertedPed) {
            convertedPed.ragdollRequest = default;
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

        public static void RequestFacialIdleAnim(this ref ConvertedPed convertedPed, string facialAnimName) {
            if (!convertedPed.facialIdleAnimLock) {
                convertedPed.facialIdleAnim = facialAnimName;
                convertedPed.facialIdleAnimApplied = false;
            }
        }

        public static void ResetFacialIdleAnim(this ref ConvertedPed convertedPed) {
            if (!convertedPed.facialIdleAnimLock) {
                convertedPed.facialIdleAnim = null;
                convertedPed.facialIdleAnimApplied = false;
            }
        }

        // public static bool HasForcedAnimation(this in ConvertedPed convertedPed) {
        //     (string, string) animInfo = convertedPed.forcedAnimation;
        //     return !string.IsNullOrEmpty(animInfo.Item1) && !string.IsNullOrEmpty(animInfo.Item2);
        // }

        public static bool IsAbleToDoSomething(this in ConvertedPed convertedPed) {
            return convertedPed.thisPed.CurrentVehicle != null
                    ? !convertedPed.isRestrictToDrive
                    : convertedPed.thisPed.StandsStill();
        }

        public static bool IsInEmergencyVehicle(this in ConvertedPed convertedPed, out Vehicle vehicle) {
            if (!convertedPed.thisPed.IsInVehicle()) {
                vehicle = null;
                return false;
            }

            vehicle = convertedPed.thisPed.CurrentVehicle;
            return vehicle != null && vehicle.ClassType == VehicleClass.Emergency;
        }
    }
}