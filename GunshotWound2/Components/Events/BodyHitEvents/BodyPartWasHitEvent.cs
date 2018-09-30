namespace GunshotWound2.Components.Events.BodyHitEvents
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