namespace GunshotWound2.PedsFeature {
    using GTA;
    using GTA.Native;
    using SHVDN;

    public static class PedEffects {
        public static bool TryRequestMoveSet(string moveSetName) {
            if (string.IsNullOrEmpty(moveSetName)) {
                return false;
            }

            var isLoaded = Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, moveSetName);
            if (!isLoaded) {
                Function.Call(Hash.REQUEST_ANIM_SET, moveSetName);
            }

            return isLoaded;
        }

        public static void ChangeMoveSet(Ped ped, string moveSetName) {
            if (string.IsNullOrEmpty(moveSetName)) {
                ResetMoveSet(ped);
            } else {
                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, ped, moveSetName, 1f);
            }
        }

        public static void ResetMoveSet(Ped ped) {
            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, ped, 1f);
        }

        /// <summary>
        /// Min: 0.00
        /// Max: 10.00
        /// Needs to be looped!  
        /// </summary>
        public static void OverrideMoveRate(Ped ped, float moveRate) {
            Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped, moveRate);
        }

        public static void SetNaturalMotionMessage(Ped ped, int messageId) {
            Function.Call(Hash.CREATE_NM_MESSAGE, true, messageId);
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, ped);
        }

        public static void StopNaturalMotion(Ped ped) {
            SetNaturalMotionMessage(ped, 0);
        }

        public static void ClearBloodDamage(Ped ped) {
            Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, ped);
        }

        public static void PlayFacialAnim(Ped ped, string animation, bool useMaleDict) {
            string animDict = useMaleDict ? "facials@gen_male@base" : "facials@gen_female@base";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            Function.Call(Hash.PLAY_FACIAL_ANIM, ped, animation, animDict);
        }

        public static void SetFacialIdleAnim(Ped ped, string animation, bool useMaleDict) {
            string animDict = useMaleDict ? "facials@gen_male@base" : "facials@gen_female@base";
            Function.Call(Hash.REQUEST_ANIM_DICT, animDict);
            Function.Call(Hash.SET_FACIAL_CLIPSET, ped, animDict);
            Function.Call(Hash.SET_FACIAL_IDLE_ANIM_OVERRIDE, ped, animation, 0);
        }

        public static void CleanFacialIdleAnim(Ped ped) {
            Function.Call(Hash.RESET_FACIAL_IDLE_ANIM, ped);
            Function.Call(Hash.CLEAR_FACIAL_IDLE_ANIM_OVERRIDE, ped);
        }

        /// <summary>
        /// Works for both player and peds, but some flags don't seem to work for the player (1, for example)  
        /// 1 - Blocks ragdolling when shot.  
        /// 2 - Blocks ragdolling when hit by a vehicle. The ped still might play a falling animation.  
        /// 4 - Blocks ragdolling when set on fire.
        /// </summary>
        public static void SetRagdollBlockingFlags(Ped ped, int flag) {
            Function.Call(Hash.SET_RAGDOLL_BLOCKING_FLAGS, ped, flag);
        }

        public static void SetVehicleOutOfControl(Vehicle vehicle) {
            Function.Call(Hash.SET_VEHICLE_OUT_OF_CONTROL, vehicle, false, false);
        }

        public static void StartWritheTask(Ped ped, Ped target = null) {
            ped.Health = 200;
            Function.Call(Hash.TASK_WRITHE, ped, target ?? ped, -1, 0, true, 0);
        }

        public static bool IsPedInWrithe(Ped ped) {
            return Function.Call<bool>(Hash.IS_PED_IN_WRITHE, ped);
        }

        public static bool TryGetLastDamageRecord(Ped ped, out uint weaponHash, out int attackerHandle) {
            var record = NativeMemory.GetEntityDamageRecordEntryAtIndex(ped.MemoryAddress, 0);
            weaponHash = unchecked((uint)record.weaponHash);
            attackerHandle = record.attackerEntityHandle;
            return record.gameTime != 0;
        }
    }
}