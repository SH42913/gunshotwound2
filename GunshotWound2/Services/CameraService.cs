namespace GunshotWound2.Services {
    using System.Collections.Generic;
    using GTA;
    using GTA.Native;
    using Utils;

    public sealed class CameraService {
        private const int HEAD_INJURY_PRIORITY = 0;
        private const int UNCONSCIOUS_PRIORITY = 1;
        private const int PAINKILLERS_PRIORITY = 2;
        private const int HEART_INJURY_PRIORITY = 3;
        private const int LUNGS_INJURY_PRIORITY = 4;
        private const int BLEED_EFFECT_PRIORITY = 5;
        private const int PAIN_EFFECT_PRIORITY = 6;

        private readonly ILogger logger;

        private readonly SortedList<int, string> postFxList = new(7);

        public bool useScreenEffects;
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

        public void SetHeadInjuryEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetHeadInjuryEffect)} with {value.ToString()}");
#endif
            SetPostFx(HEAD_INJURY_PRIORITY, "DeathFailMPIn", value, false);
        }

        public void SetUnconsciousEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetUnconsciousEffect)} with {value.ToString()}");
#endif
            SetPostFx(UNCONSCIOUS_PRIORITY, "DeathFailOut", value, true);
        }

        public void SetPainkillersEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetPainkillersEffect)} with {value.ToString()}");
#endif
            SetPostFx(PAINKILLERS_PRIORITY, "InchPickup", value, true);
        }

        public void SetHeartInjuryEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetHeartInjuryEffect)} with {value.ToString()}");
#endif
            SetPostFx(HEART_INJURY_PRIORITY, "DrugsDrivingIn", value, false);
        }

        public void SetLungsInjuryEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetLungsInjuryEffect)} with {value.ToString()}");
#endif
            SetPostFx(LUNGS_INJURY_PRIORITY, "LostTimeNight", value, true);
        }

        public void SetHeavyBleedingEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetHeavyBleedingEffect)} with {value.ToString()}");
#endif
            SetPostFx(BLEED_EFFECT_PRIORITY, "CrossLine", value, true);
        }

        public void SetPainEffect(bool value) {
#if DEBUG
            logger.WriteInfo($"{nameof(SetPainEffect)} with {value.ToString()}");
#endif
            SetPostFx(PAIN_EFFECT_PRIORITY, "FocusIn", value, false);
        }

        public void ClearAllEffects() {
            aimingShakeCritType = false;
            aimingShakeAmplitude = 0f;
            SetAimingShake(needShaking: false);
            StopAllPostFx();
        }

        private void SetPostFx(int priority, string name, bool value, bool loop) {
            string prev = null;
            if (postFxList.Count > 0) {
                prev = postFxList.Values[0];
            }

            bool isActive = postFxList.ContainsKey(priority);
            if (isActive != value) {
                if (value) {
                    postFxList.Add(priority, name);
                } else {
                    postFxList.Remove(priority);
                }
            }

            string next = null;
            if (postFxList.Count > 0) {
                next = postFxList.Values[0];
            }

            if (prev == next) {
                return;
            }

#if DEBUG
            logger.WriteInfo($"Changing postFx {prev} -> {next}");
#endif

            if (useScreenEffects && !string.IsNullOrEmpty(prev)) {
                Function.Call(Hash.ANIMPOSTFX_STOP, prev);
            }

            if (useScreenEffects && !string.IsNullOrEmpty(next)) {
                const int durationInMs = 5000;
                Function.Call(Hash.ANIMPOSTFX_PLAY, next, durationInMs, loop);
            }
        }

        private void StopAllPostFx() {
#if DEBUG
            logger.WriteInfo("Stop all PostFx");
#endif
            postFxList.Clear();
            Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
        }
    }
}