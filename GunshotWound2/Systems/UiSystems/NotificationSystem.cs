using System;
using GTA;
using GunshotWound2.Components.UiComponents;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.UiSystems
{
    [EcsInject]
    public class NotificationSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        private EcsFilter<NotificationComponent> _components;

        public void Run()
        {
            GunshotWound2.LastSystem = nameof(NotificationSystem);
            
            string commonNotification = "";
            string warnNotification = "";
            string alertNotification = "";
            string emergencyNotification = "";
            string debugNotification = "";
            
            for (int i = 0; i < _components.EntitiesCount; i++)
            {
                var needToShow = _components.Components1[i];

                switch (needToShow.Level)
                {
                    case NotifyLevels.COMMON:
                        commonNotification += needToShow.StringToShow + "\n";
                        break;
                    case NotifyLevels.WARNING:
                        warnNotification += needToShow.StringToShow + "\n";
                        break;
                    case NotifyLevels.ALERT:
                        alertNotification += needToShow.StringToShow + "\n";
                        break;
                    case NotifyLevels.EMERGENCY:
                        emergencyNotification += needToShow.StringToShow + "\n";
                        break;
                    case NotifyLevels.DEBUG:
                        debugNotification += needToShow.StringToShow + "\n";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                _ecsWorld.RemoveEntity(_components.Entities[i]);
            }
            
            if (!string.IsNullOrEmpty(emergencyNotification))
            {
                UI.Notify("~r~" + emergencyNotification);
            }
            
            if (!string.IsNullOrEmpty(alertNotification))
            {
                UI.Notify("~o~" + alertNotification);
            }
            
            if (!string.IsNullOrEmpty(warnNotification))
            {
                UI.Notify("~y~" + warnNotification);
            }
            
            if (!string.IsNullOrEmpty(commonNotification))
            {
                UI.Notify(commonNotification);
            }

#if DEBUG
            if (!string.IsNullOrEmpty(debugNotification))
            {
                UI.Notify("Debug:\n~c~" + debugNotification);
            }
#endif
        }
    }
}