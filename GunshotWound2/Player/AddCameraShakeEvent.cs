namespace GunshotWound2.Player
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