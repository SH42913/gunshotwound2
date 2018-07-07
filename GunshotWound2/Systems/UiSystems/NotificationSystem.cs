using System;
using GTA;
using GunshotWoundEcs.Components.UiComponents;
using GunshotWoundEcs.Configs;
using LeopotamGroup.Ecs;

namespace GunshotWoundEcs.Systems.UiSystems
{
    [EcsInject]
    public class NotificationSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        private EcsFilter<NotificationComponent> _components;

        public void Run()
        {
            GunshotWoundScript.LastSystem = nameof(NotificationSystem);
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

            if (_mainConfig.Data.Debug && !string.IsNullOrEmpty(debugNotification))
            {
                UI.Notify("Debug:\n~c~" + debugNotification);
            }
            
            if (!string.IsNullOrEmpty(commonNotification))
            {
                UI.Notify(commonNotification);
            }
            
            if (!string.IsNullOrEmpty(warnNotification))
            {
                UI.Notify("~y~" + warnNotification);
            }
            
            if (!string.IsNullOrEmpty(alertNotification))
            {
                UI.Notify("~o~" + alertNotification);
            }
            
            if (!string.IsNullOrEmpty(emergencyNotification))
            {
                UI.Notify("~r~" + emergencyNotification);
            }
        }
    }
}