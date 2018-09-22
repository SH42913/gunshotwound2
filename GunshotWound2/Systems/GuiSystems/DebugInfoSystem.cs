using GunshotWound2.Components.Events.GuiEvents;
using GunshotWound2.Components.StateComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.GuiSystems
{
    [EcsInject]
    public class DebugInfoSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        private EcsFilter<ShowDebugInfoEvent> _events;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(DebugInfoSystem);
#endif

            for (int i = 0; i < _events.EntitiesCount; i++)
            {
#if DEBUG
                int pedEntity = _events.Components1[i].Entity;
                if (!_ecsWorld.IsEntityExists(pedEntity)) continue;

                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);
                if (woundedPed == null) continue;
                
                SendDebug($"{woundedPed}");
                _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().Entity = pedEntity;
#endif
            }
            _events.RemoveAllEntities();
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