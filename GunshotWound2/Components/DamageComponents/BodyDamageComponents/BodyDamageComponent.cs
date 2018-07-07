namespace GunshotWound2.Components.DamageComponents.BodyDamageComponents
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
    
    public class BodyDamageComponent
    {
        public int PedEntity;
        public BodyParts DamagedPart;
    }
}