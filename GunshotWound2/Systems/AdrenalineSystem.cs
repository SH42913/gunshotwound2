using GTA;
using GunshotWound2.Components;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems
{
    [EcsInject]
    public class AdrenalineSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilter<AdrenalineComponent> _components;
        private EcsFilterSingle<MainConfig> _config;
        private float _currentTimeScale = 1f;
        private float _minimalTimeScale = 0.6f;
        private float _increaseStep = 0.1f;
        private float _stabSpeed = 0.001f;
        
        public void Run()
        {
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                if (_components.Components1[i].Revert) _currentTimeScale = 1f;
                _ecsWorld.RemoveEntity(_components.Entities[i]);
                
                if(!_config.Data.PlayerConfig.AdrenalineSlowMotion) continue;
                if(_currentTimeScale > _minimalTimeScale)
                {
                    _currentTimeScale -= _increaseStep;
                    SendMessage($"New TIME scale: {_currentTimeScale}");
                }
                else
                {
                    _currentTimeScale = _minimalTimeScale;
                }
            }

            if (_currentTimeScale > 1) _currentTimeScale = 1;
            if (_currentTimeScale >= 1) return;
            Game.TimeScale = _currentTimeScale;
            _currentTimeScale += _stabSpeed * Game.LastFrameTime;
        }

        private void SendMessage(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}