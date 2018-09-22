namespace GunshotWound2.Components.StateComponents
{
    public class BleedingComponent : ComponentWithEntity
    {
        public const float MAX_SEVERITY_FOR_HEAL = 1f;
        
        public float BleedSeverity;
        public string Name;
        public bool CanBeHealed;
    }
}