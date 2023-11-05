namespace GunshotWound2.Effects {
    using GTA.Native;

    public static class CameraEffects {
        public static void ShakeCameraOnce() {
            Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 1);
        }

        public static void ShakeCameraPermanent() {
            Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 2);
        }

        public static void ClearCameraShake() {
            Function.Call(Hash.SET_CAM_DEATH_FAIL_EFFECT_STATE, 0);
        }

        public static void FlashCameraOnce() {
            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
        }

        public static void StartPostFx(string animation, int durationInMs, bool loop) {
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