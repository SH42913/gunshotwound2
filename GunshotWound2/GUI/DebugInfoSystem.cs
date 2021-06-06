using GunshotWound2.HitDetection;
using GunshotWound2.Utils;
using Leopotam.Ecs;

namespace GunshotWound2.GUI
{
    [EcsInject]
    public sealed class DebugInfoSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilter<ShowDebugInfoEvent> _events = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(DebugInfoSystem);
#endif

            for (var i = 0; i < _events.EntitiesCount; i++)
            {
#if DEBUG
                var pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;

                SendDebug($"{woundedPed}");
                _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = pedEntity;
#endif
            }

            _events.CleanFilter();
        }

        private void SendDebug(string message)
        {
#if DEBUG
            var notification = _ecsWorld.CreateEntityWith<ShowNotificationEvent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
#endif
        }
    }
}