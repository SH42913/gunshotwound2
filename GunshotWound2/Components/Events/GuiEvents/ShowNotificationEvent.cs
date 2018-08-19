namespace GunshotWound2.Components.Events.GuiEvents
{
    public enum NotifyLevels
    {
        COMMON,
        WARNING,
        ALERT,
        EMERGENCY,
        DEBUG
    }
    
    public class ShowNotificationEvent
    {
        public NotifyLevels Level;
        public string StringToShow;
    }
}