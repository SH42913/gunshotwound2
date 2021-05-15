using GTA.Native;
using GunshotWound2.HitDetection;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Effects
{
    [EcsInject]
    public sealed class SwitchAnimationSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<ChangeWalkAnimationEvent> _events = null;

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
                if (woundedPed == null) continue;

                var animationName = _events.Components1[i].AnimationName;
                if (string.IsNullOrEmpty(animationName) || !woundedPed.ThisPed.IsAlive) continue;

                Function.Call(Hash.REQUEST_ANIM_SET, animationName);
                if (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, animationName))
                {
                    Function.Call(Hash.REQUEST_ANIM_SET, animationName);
                }

                Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, woundedPed.ThisPed, animationName, 1.0f);
            }

            _events.CleanFilter();
        }
    }
}