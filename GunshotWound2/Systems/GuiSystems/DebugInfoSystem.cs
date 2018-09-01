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
        private EcsFilter<ShowDebugInfoEvent> _components;
        
        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(DebugInfoSystem);
#endif
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
#if DEBUG
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    SendDebug($"{woundedPed}");
                    _ecsWorld.CreateEntityWith<ShowHealthStateEvent>().PedEntity = pedEntity;
                }
#endif
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
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