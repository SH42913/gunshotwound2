namespace GunshotWound2.Components.Events.PedEvents
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
    
    public class SetPedToRagdollEvent : ComponentWithEntity
    {
        public RagdollStates RagdollState;
    }
}