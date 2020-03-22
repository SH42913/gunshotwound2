namespace GunshotWound2.Player
{
    public enum CameraShakeLength
    {
        ONE_TIME,
        PERMANENT,
        CLEAR
    }

    public sealed class AddCameraShakeEvent
    {
        public CameraShakeLength Length;
    }
}