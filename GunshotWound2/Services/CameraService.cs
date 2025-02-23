namespace GunshotWound2.Services {
    using System.Collections.Generic;
    using GTA;
    using GTA.Native;
    using Utils;

    public sealed class CameraService {
        private readonly ILogger logger;

        private readonly SortedList<int, string> postFxList = new(4);

        public float aimingShakeAmplitude;
        public bool aimingShakeCritType;

        public CameraService(ILogger logger) {
            this.logger = logger;
        }

        public void SetAimingShake(bool needShaking) {
            bool isShaking = GameplayCamera.IsShaking;
            if (isShaking == needShaking) {
                return;
            }

            if (needShaking) {
                CameraShake type = aimingShakeCritType ? CameraShake.Drunk : CameraShake.Hand;
                float amplitude = aimingShakeAmplitude >= 0f ? aimingShakeAmplitude : 0f;
#if DEBUG
                logger.WriteInfo($"Starting aiming shake type {type} with {amplitude.ToString("F2")} amplitude");
#endif
                GameplayCamera.Shake(type, amplitude);
            } else {
                GameplayCamera.StopShaking();
            }
        }

        public void PlayPainfulWoundEffect() {
#if DEBUG
            logger.WriteInfo(nameof(PlayPainfulWoundEffect));
#endif
            Function.Call(Hash.SET_FLASH, 0, 0, 100, 500, 100);
        }

        public void SetUnconsciousEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetUnconsciousEffect)} with {value.ToString()}");
#endif
            SetPostFx(priority: 1, "DeathFailOut", value);
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

        public void SetBleedingEffect(bool value) {
            const int priority = 4;
            if (postFxList.ContainsKey(priority) != value) {
#if DEBUG
                logger.WriteInfo($"{nameof(SetBleedingEffect)} with {value.ToString()}");
#endif
                SetPostFx(priority, "CrossLine", value);
            }
        }

        public void SetPainEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetPainEffect)} with {value.ToString()}");
#endif
            SetPostFx(priority: 5, "FocusIn", value);
        }

        public void ClearAllEffects() {
            aimingShakeCritType = false;
            aimingShakeAmplitude = 0f;
            SetAimingShake(needShaking: false);
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
#if DEBUG
            logger.WriteInfo($"Stop all PostFx");
#endif
            postFxList.Clear();
            Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
        }
    }
}