using GTA;
using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.HitDetection;
using Leopotam.Ecs;

namespace GunshotWound2.Pain
{
    [EcsInject]
    public sealed class PainRecoverySystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<WoundedPedComponent, PainComponent> _pedsWithPain = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;
        private static readonly InputArgument[] FacialAnimArguments = new InputArgument[3];

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(PainRecoverySystem);
#endif
            var frameTimeInSeconds = Game.LastFrameTime;

            for (var i = 0; i < _pedsWithPain.EntitiesCount; i++)
            {
                var woundedPed = _pedsWithPain.Components1[i];
                var pain = _pedsWithPain.Components2[i];

                var pedEntity = _pedsWithPain.Entities[i];
                if (pain.CurrentPain <= 0f || woundedPed.IsDead)
                {
                    _ecsWorld.RemoveComponent<PainComponent>(pedEntity, true);
                    continue;
                }

                pain.CurrentPain -= woundedPed.PainRecoverSpeed * frameTimeInSeconds;
                var painPercent = pain.CurrentPain / woundedPed.MaximalPain;
                var backPercent = painPercent > 1
                    ? 0
                    : 1 - painPercent;

                if (painPercent > 3f)
                {
                    if (woundedPed.PainState == PainStates.DEADLY) continue;

                    _ecsWorld.CreateEntityWith(out DeadlyPainChangeStateEvent deadlyPainEvent);
                    deadlyPainEvent.Entity = pedEntity;
                    deadlyPainEvent.ForceUpdate = false;
                }
                else if (painPercent > 1f)
                {
                    PlayFacialAnim(woundedPed, "dead_1");

                    if (woundedPed.PainState == PainStates.UNBEARABLE) continue;

                    _ecsWorld.CreateEntityWith(out UnbearablePainChangeStateEvent unbearablePainEvent);
                    unbearablePainEvent.Entity = pedEntity;
                    unbearablePainEvent.ForceUpdate = false;
                }
                else if (painPercent > 0.8f)
                {
                    PlayFacialAnim(woundedPed, "mood_injured_1");

                    if (woundedPed.PainState == PainStates.INTENSE) continue;

                    _ecsWorld.CreateEntityWith(out IntensePainChangeStateEvent intensePainEvent);
                    intensePainEvent.Entity = pedEntity;
                    intensePainEvent.ForceUpdate = false;
                }
                else if (painPercent > 0.3f)
                {
                    if (woundedPed.PainState == PainStates.AVERAGE) continue;

                    _ecsWorld.CreateEntityWith(out AveragePainChangeStateEvent averagePainEvent);
                    averagePainEvent.Entity = pedEntity;
                    averagePainEvent.ForceUpdate = false;
                }
                else if (painPercent > 0.01f)
                {
                    if (woundedPed.PainState == PainStates.MILD) continue;

                    _ecsWorld.CreateEntityWith(out MildPainChangeStateEvent mildPainEvent);
                    mildPainEvent.Entity = pedEntity;
                    mildPainEvent.ForceUpdate = false;
                }
                else
                {
                    PlayFacialAnim(woundedPed, "mood_happy_1");

                    if (woundedPed.PainState == PainStates.NONE) continue;

                    _ecsWorld.CreateEntityWith(out NoPainChangeStateEvent noPainEvent);
                    noPainEvent.Entity = pedEntity;
                    noPainEvent.ForceUpdate = false;

                    if (!_config.Data.PlayerConfig.CameraIsShaking)
                    {
                        Function.Call(Hash._SET_CAM_EFFECT, 0);
                        Function.Call(Hash.ANIMPOSTFX_STOP_ALL);
                    }
                }

                if (woundedPed.Crits.Has(CritTypes.LEGS_DAMAGED)) continue;

                var adjustable = 1f - _config.Data.WoundConfig.MoveRateOnFullPain;
                var moveRate = 1f - adjustable * backPercent;
                Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, woundedPed.ThisPed, moveRate);
            }
        }

        public static void PlayFacialAnim(WoundedPedComponent woundedPed, string animation)
        {
            FacialAnimArguments[0] = woundedPed.ThisPed;
            FacialAnimArguments[1] = animation;
            FacialAnimArguments[2] = woundedPed.IsMale ? "facials@gen_male@base" : "facials@gen_female@base";
            Function.Call(Hash.PLAY_FACIAL_ANIM, FacialAnimArguments);
            FacialAnimArguments[0] = null;
        }
    }
}