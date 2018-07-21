using GunshotWound2.Components.UiComponents;
using GunshotWound2.Components.WoundComponents;
using GunshotWound2.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWound2.Systems.UiSystems
{
    [EcsInject]
    public class DebugInfoSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        private EcsFilter<DebugInfoRequestComponent> _components;
        
        public void Run()
        {
            GunshotWound2.LastSystem = nameof(DebugInfoSystem);
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
#if DEBUG
                int pedEntity = _components.Components1[i].PedEntity;
                var woundedPed = _ecsWorld.GetComponent<WoundedPedComponent>(pedEntity);

                if (woundedPed != null)
                {
                    SendDebug($"{woundedPed}");
                    _ecsWorld.CreateEntityWith<CheckPedComponent>().PedEntity = pedEntity;
                }
#endif
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
        }

        private void SendDebug(string message)
        {
            var notification = _ecsWorld.CreateEntityWith<NotificationComponent>();
            notification.Level = NotifyLevels.DEBUG;
            notification.StringToShow = message;
        }
    }
}