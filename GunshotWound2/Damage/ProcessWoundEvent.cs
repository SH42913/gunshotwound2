using GunshotWound2.HitDetection;
using GunshotWound2.Utils;

namespace GunshotWound2.Damage
{
    public sealed class ProcessWoundEvent : ComponentWithEntity
    {
        public float Damage;
        public float BleedSeverity;
        public float Pain;
        public bool ArterySevered;
        public CritTypes? Crits;
        public string Name;
    }
}