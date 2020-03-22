using System;
using GTA;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.GUI
{
    [EcsInject]
    public sealed class NotificationSystem : IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private EcsFilterSingle<MainConfig> _mainConfig;
        private EcsFilter<ShowNotificationEvent> _events;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(NotificationSystem);
#endif

            string commonNotification = "";
            string warnNotification = "";
            string alertNotification = "";
            string emergencyNotification = "";
            string debugNotification = "";

            for (int i = 0; i < _events.EntitiesCount; i++)
            {
                var needToShow = _events.Components1[i];

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

                _ecsWorld.RemoveEntity(_events.Entities[i]);
            }

            if (_mainConfig.Data.EmergencyMessages && !string.IsNullOrEmpty(emergencyNotification))
            {
                UI.Notify("~r~" + emergencyNotification);
            }

            if (_mainConfig.Data.AlertMessages && !string.IsNullOrEmpty(alertNotification))
            {
                UI.Notify("~o~" + alertNotification);
            }

            if (_mainConfig.Data.WarningMessages && !string.IsNullOrEmpty(warnNotification))
            {
                UI.Notify("~y~" + warnNotification);
            }

            if (_mainConfig.Data.CommonMessages && !string.IsNullOrEmpty(commonNotification))
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