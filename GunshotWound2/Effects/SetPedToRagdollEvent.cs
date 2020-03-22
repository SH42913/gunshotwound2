using GunshotWound2.Utils;

namespace GunshotWound2.Effects
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