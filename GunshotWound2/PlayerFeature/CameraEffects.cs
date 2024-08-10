namespace GunshotWound2.PlayerFeature {
    using GTA.Native;

    public static class CameraEffects {
        private static bool PERMANENT_SHAKING;

        public static void ShakeCameraOnce() {
            if (!PERMANENT_SHAKING) {
                Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 1);
            }
        }

        public static void ShakeCameraPermanent() {
            if (!PERMANENT_SHAKING) {
                Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 2);
                PERMANENT_SHAKING = true;
            }
        }

        public static void ClearCameraShake() {
            Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 0);
            PERMANENT_SHAKING = false;
        }

        public static void FlashCameraOnce() {
            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
        }

        public static void StartPostFx(string animation, int durationInMs, bool loop = true) {
            Function.Call(Hash.ANIMPOSTFX_PLAY, animation, durationInMs, loop);
        }

        public static void StopPostFx(string animation) {
            Function.Call(Hash.ANIMPOSTFX_STOP, animation);
        }

        public static void StopAllPostFx() {
            Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
        }
    }
}