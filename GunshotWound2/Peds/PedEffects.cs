namespace GunshotWound2.Peds {
    using System;
    using GTA;
    using GTA.Native;

    public static class PedEffects {
        public static void ChangeMoveSetRandom(Ped ped, string[] moveSets, Random random) {
            string set = moveSets != null && moveSets.Length > 0
                    ? moveSets[random.Next(0, moveSets.Length)]
                    : null;

            ChangeMoveSet(ped, set);
        }

        public static void ChangeMoveSet(Ped ped, string moveSetName) {
            if (string.IsNullOrEmpty(moveSetName)) {
                ResetMoveSet(ped);
                return;
            }

            if (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, moveSetName)) {
                Function.Call(Hash.REQUEST_ANIM_SET, moveSetName);
            }

            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, ped, moveSetName, 1f);
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
            //Possibly needs to be looped
            Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped, moveRate);
        }

        /// <summary>
        /// Ragdoll Types
        /// 0: CTaskNMRelax
        /// 1: CTaskNMScriptControl: Hardcoded not to work in networked environments.
        /// Else: CTaskNMBalance
        /// </summary>
        public static void SetRagdoll(Ped ped, int timeInMs, int type = 0) {
            Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, timeInMs, timeInMs, type);
        }

        public static void SetPermanentRagdoll(Ped ped) {
            SetRagdoll(ped, timeInMs: -1);
        }

        public static void ResetRagdoll(Ped ped) {
            if (ped.IsRagdoll) {
                SetRagdoll(ped, timeInMs: 1);
            }
        }

        public static void SetNaturalMotionMessage(Ped ped, int messageId) {
            Function.Call(Hash.CREATE_NM_MESSAGE, true, messageId);
            Function.Call(Hash.GIVE_PED_NM_MESSAGE, ped);
        }

        /// <summary>
        /// 0: PAIN_*_GENERIC (Low, Medium, High)
        /// 1: UNUSED
        /// 2: UNUSED
        /// 3: SCREAM_PANIC (Nothing can be heard)
        /// 4: SCREAM_PANIC_SHORT
        /// 5: SCREAM_SCARED
        /// 6: SCREAM_SHOCKED
        /// 7: SCREAM_TERROR
        /// 8: ON_FIRE
        /// 9: UNUSED
        /// 10: UNUSED
        /// 11: INHALE (Nothing can be heard)
        /// 12: EXHALE (Nothing can be heard)
        /// 13: DEATH_HIGH_SHORT
        /// 14: UNUSED
        /// 15: PAIN_HIGH_GENERIC
        /// 16: PAIN_*_GENERIC (Low, Medium, High)
        /// 17: PAIN_SHOVE
        /// 18: PAIN_WHEEZE
        /// 19: COUGH
        /// 20: PAIN_TAZER
        /// 21: UNUSED
        /// 22: CLIMB_LARGE (Nothing can be heard)
        /// 23: CLIMB_SMALL (Nothing can be heard)
        /// 24: JUMP (Nothing can be heard)
        /// 25: COWER
        /// 26: WHIMPER
        /// 27: DYING_MOAN
        /// 28: EXHALE_CYCLING (Nothing can be heard)
        /// 29: PAIN_RAPIDS (Nothing can be heard)
        /// 30: SNEEZE
        /// 31: MELEE_SMALL_GRUNT (Nothing can be heard)
        /// 32: MELEE_LARGE_GRUNT (Nothing can be heard)
        /// 33: PAIN_*_GENERIC (Low, Medium, High)
        /// </summary>
        public static void PlayPain(Ped ped, int painId) {
            Function.Call(Hash.PLAY_PAIN, ped, painId, 0, 0);
        }

        public static void ClearBloodDamage(Ped ped) {
            Function.Call(Hash.CLEAR_PED_BLOOD_DAMAGE, ped);
        }

        public static void PlayFacialAnim(Ped ped, string animation, bool useMaleDict) {
            string animDict = useMaleDict
                    ? "facials@gen_male@base"
                    : "facials@gen_female@base";

            Function.Call(Hash.PLAY_FACIAL_ANIM, ped, animation, animDict);
        }
    }
}