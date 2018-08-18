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
    
    public class BodyPartWasHitEvent : ComponentWithPedEntity
    {
        public BodyParts DamagedPart;
    }
}