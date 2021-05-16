using System;
using GTA.UI;
using GunshotWound2.Configs;
using Leopotam.Ecs;

namespace GunshotWound2.GUI
{
    [EcsInject]
    public sealed class NotificationSystem : IEcsRunSystem
    {
        private readonly EcsWorld _ecsWorld = null;
        private readonly EcsFilterSingle<MainConfig> _mainConfig = null;
        private readonly EcsFilter<ShowNotificationEvent> _events = null;

        public void Run()
        {
#if DEBUG
            GunshotWound2.LastSystem = nameof(NotificationSystem);
#endif

            var commonNotification = "";
            var warnNotification = "";
            var alertNotification = "";
            var emergencyNotification = "";
            var debugNotification = "";

            for (var i = 0; i < _events.EntitiesCount; i++)
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
                Notification.Show("~r~" + emergencyNotification);
            }

            if (_mainConfig.Data.AlertMessages && !string.IsNullOrEmpty(alertNotification))
            {
                Notification.Show("~o~" + alertNotification);
            }

            if (_mainConfig.Data.WarningMessages && !string.IsNullOrEmpty(warnNotification))
            {
                Notification.Show("~y~" + warnNotification);
            }

            if (_mainConfig.Data.CommonMessages && !string.IsNullOrEmpty(commonNotification))
            {
                Notification.Show(commonNotification);
            }

#if DEBUG
            if (!string.IsNullOrEmpty(debugNotification))
            {
                Notification.Show("Debug:\n~c~" + debugNotification);
            }
#endif
        }
    }
}