namespace GunshotWound2.Components.WoundComponents
{
    public class WoundComponent : ComponentWithPedEntity
    {
        public float Damage;
        public float BleedSeverity;
        public float Pain;
        public bool ArterySevered;
        public DamageTypes? CriticalDamage;
        public string Name;
    }
}