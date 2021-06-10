using GTA.Native;
using GunshotWound2.HitDetection;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Effects
{
    [EcsInject]
    public sealed class MoveSetSwitchSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<SwitchMoveSetRequest> _events = null;

        private readonly InputArgument[] _animRequestParams = new InputArgument[1];
        private readonly InputArgument[] _animSetParams = new InputArgument[3];

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(SwitchAnimationSystem);
#endif

            for (var i = 0; i < _events.EntitiesCount; i++)
            {
                var pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null || !woundedPed.ThisPed.IsAlive) continue;

                var animationName = _events.Components1[i].AnimationName;
                if (string.IsNullOrEmpty(animationName))
                {
                    Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, woundedPed.ThisPed, 0f);
                    continue;
                }

                _animRequestParams[0] = animationName;
                if (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, _animRequestParams))
                {
                    Function.Call(Hash.REQUEST_ANIM_SET, _animRequestParams);
                }

                _animSetParams[0] = woundedPed.ThisPed;
                _animSetParams[1] = animationName;
                _animSetParams[2] = 1.0f;
                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, _animSetParams);
            }

            _events.CleanFilter();
        }
    }
}