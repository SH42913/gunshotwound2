using System;
using GTA.Native;
using GunshotWound2.Configs;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.Player
{
    [EcsInject]
    public sealed class CameraShakeSystem : IEcsRunSystem
    {
        private readonly EcsFilter<AddCameraShakeEvent> _events = null;
        private readonly EcsFilterSingle<MainConfig> _config = null;

        public void Run()
        {
            for (var i = 0; i < _events.EntitiesCount; i++)
            {
                var newEvent = _events.Components1[i];

                switch (newEvent.Length)
                {
                    case CameraShakeLength.ONE_TIME:
                        if (_config.Data.PlayerConfig.CameraIsShaking) continue;
                        Function.Call(Hash._SET_CAM_EFFECT, 1);
                        _config.Data.PlayerConfig.CameraIsShaking = false;
                        break;
                    case CameraShakeLength.PERMANENT:
                        if (_config.Data.PlayerConfig.CameraIsShaking) continue;
                        Function.Call(Hash._SET_CAM_EFFECT, 2);
                        _config.Data.PlayerConfig.CameraIsShaking = true;
                        break;
                    case CameraShakeLength.CLEAR:
                        if (!_config.Data.PlayerConfig.CameraIsShaking) continue;
                        Function.Call(Hash._SET_CAM_EFFECT, 0);
                        _config.Data.PlayerConfig.CameraIsShaking = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _events.CleanFilter();
        }
    }
}