namespace GunshotWound2.Components.HitComponents.BodyDamageComponents
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
    
    public class BodyHitComponent : ComponentWithPedEntity
    {
        public BodyParts DamagedPart;
    }
}