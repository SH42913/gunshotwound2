namespace GunshotWoundEcs.Components.UiComponents
{
    public enum NotifyLevels
    {
        COMMON,
        WARNING,
        ALERT,
        EMERGENCY,
        DEBUG
    }
    
    public class NotificationComponent
    {
        public NotifyLevels Level;
        public string StringToShow;
    }
}