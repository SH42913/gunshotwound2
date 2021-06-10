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
        HEART_DAMAGE,
        GUTS_DAMAGE,
    }

    public sealed class SetPedToRagdollEvent : ComponentWithEntity
    {
        public RagdollStates RagdollState;
    }
}