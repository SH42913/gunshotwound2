using GunshotWound2.Components.StateComponents;

namespace GunshotWound2.Components.Events.WoundEvents
{
    public class ProcessWoundEvent : ComponentWithPedEntity
    {
        public float Damage;
        public float BleedSeverity;
        public float Pain;
        public bool ArterySevered;
        public DamageTypes? CriticalDamage;
        public string Name;
    }
}