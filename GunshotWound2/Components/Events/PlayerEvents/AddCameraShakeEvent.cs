namespace GunshotWound2.Components.Events.PlayerEvents
{
    public enum CameraShakeLength
    {
        ONE_TIME,
        PERMANENT,
        CLEAR
    }
    
    public class AddCameraShakeEvent
    {
        public CameraShakeLength Length;
    }
}