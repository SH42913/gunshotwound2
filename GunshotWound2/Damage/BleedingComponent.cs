using GunshotWound2.Utils;

namespace GunshotWound2.Damage
{
    public sealed class BleedingComponent : ComponentWithEntity
    {
        public const float MaxSeverityForHeal = 1f;

        public float BleedSeverity;
        public string Name;
        public bool CanBeHealed;
    }
}