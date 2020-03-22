using GunshotWound2.Utils;

namespace GunshotWound2.HitDetection
{
    public enum BodyParts
    {
        HEAD,
        NECK,
        UPPER_BODY,
        LOWER_BODY,
        ARM,
        LEG,
        NOTHING
    }

    public class BodyPartWasHitEvent : ComponentWithEntity
    {
        public BodyParts DamagedPart;
    }
}