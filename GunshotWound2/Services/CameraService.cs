namespace GunshotWound2.Services {
    using System.Collections.Generic;
    using GTA;
    using GTA.Native;
    using Utils;

    public sealed class CameraService {
        private readonly ILogger logger;

        private readonly SortedList<int, string> postFxList = new(4);
        private float cameraShakeAmplitude;
        private bool cameraShakeArmsCrit;

        public float CameraShakeAmplitude {
            get => cameraShakeAmplitude;
            set => SetCameraShake(cameraShakeArmsCrit, value);
        }

        public bool CameraShakeArmsCrit {
            get => cameraShakeArmsCrit;
            set => SetCameraShake(value, cameraShakeAmplitude);
        }

        public CameraService(ILogger logger) {
            this.logger = logger;
        }

        private void SetCameraShake(bool armsCritType, float amplitude) {
#if DEBUG
            logger.WriteInfo($"Changed camera shake, armsCrit = {armsCritType}, amplitude = {amplitude.ToString("F1")}");
#endif
            bool typeChanged = cameraShakeArmsCrit != armsCritType;
            if (typeChanged) {
                cameraShakeArmsCrit = armsCritType;
            }

            cameraShakeAmplitude = amplitude;
            if (cameraShakeAmplitude <= 0f) {
                GameplayCamera.StopShaking();
                return;
            }

            if (typeChanged || !GameplayCamera.IsShaking) {
                CameraShake type = cameraShakeArmsCrit ? CameraShake.FamilyDrugTrip : CameraShake.Hand;
                GameplayCamera.Shake(type, cameraShakeAmplitude);
            } else {
                GameplayCamera.ShakeAmplitude = cameraShakeAmplitude;
            }
        }

        public void PlayPainfulWoundEffect() {
#if DEBUG
            logger.WriteInfo(nameof(PlayPainfulWoundEffect));
#endif
            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
        }

        public void SetHeartCritEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetHeartCritEffect)} with {value.ToString()}");
#endif
            SetPostFx(priority: 2, "DrugsDrivingIn", value);
        }

        public void SetLungsCritEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetLungsCritEffect)} with {value.ToString()}");
#endif
            SetPostFx(priority: 3, "LostTimeNight", value);
        }

        public void SetPainEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetPainEffect)} with {value.ToString()}");
#endif
            SetPostFx(priority: 4, "CrossLine", value);
        }

        public void SetUnconsciousEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetUnconsciousEffect)} with {value.ToString()}");
#endif
            SetPostFx(priority: 1, "DeathFailOut", value);
        }

        public void ClearAllEffects() {
            SetCameraShake(armsCritType: false, amplitude: 0f);
            StopAllPostFx();
        }

        private void SetPostFx(int priority, string name, bool value) {
            string prev = null;
            if (postFxList.Count > 0) {
                prev = postFxList.Values[0];
            }

            if (value) {
                postFxList.Add(priority, name);
            } else {
                postFxList.Remove(priority);
            }

            string next = null;
            if (postFxList.Count > 0) {
                next = postFxList.Values[0];
            }

#if DEBUG
            logger.WriteInfo($"Changing postFx {prev} -> {next}");
#endif
            if (prev == next) {
                return;
            }

            if (!string.IsNullOrEmpty(prev)) {
                Function.Call(Hash.ANIMPOSTFX_STOP, prev);
            }

            if (!string.IsNullOrEmpty(next)) {
                const int durationInMs = 5000;
                const bool loop = true;
                Function.Call(Hash.ANIMPOSTFX_PLAY, next, durationInMs, loop);
            }
        }

        private void StopAllPostFx() {
            Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
        }
    }
}