namespace GunshotWound2.Components.EffectComponents
{
    public enum RagdollStates
    {
        PERMANENT,
        WAKE_UP,
        SHORT,
        LONG,
        LEG_DAMAGE,
        HEART_DAMAGE
    }
    
    public class RagdollRequestComponent : ComponentWithPedEntity
    {
        public RagdollStates RagdollState;
    }
}