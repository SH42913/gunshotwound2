using GunshotWound2.Utils;

namespace GunshotWound2.HitDetection
{
    public enum BodyParts
    {
        NOTHING,
        HEAD,
        NECK,
        UPPER_BODY,
        LOWER_BODY,
        ARM,
        LEG,
    }

    public sealed class BodyPartWasHitEvent : ComponentWithEntity
    {
        public BodyParts DamagedPart;
    }
}