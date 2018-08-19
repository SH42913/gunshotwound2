using GunshotWound2.Components.Events.WeaponHitEvents;
using GunshotWound2.Components.StateComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageProcessingSystems
{
    [EcsInject]
    public abstract class BaseImpactDamageSystem<T> : BaseDamageSystem<T>
        where T : BaseWeaponHitEvent, new()
    {
        protected void DefaultGrazeWound(int entity)
        {
            CreateWound("Light bruise", entity, DamageMultiplier * 5f,
                -1f, PainMultiplier * 15f);
        }

        protected void AbrasionWoundOn(string position, int entity)
        {
            CreateWound($"Abrasion wound on {position}", entity, DamageMultiplier * 5f,
                BleeedingMultiplier * 0.2f, PainMultiplier * 15f);
        }

        protected void LightBruiseWound(string position, int entity)
        {
            CreateWound($"Light bruise on {position}", entity, DamageMultiplier * 5f,
                -1f, PainMultiplier * 15f);
        }

        protected void MediumBruiseWound(string position, int entity)
        {
            CreateWound($"Medium bruise on {position}", entity, DamageMultiplier * 10f,
                -1f, PainMultiplier * 30f);
        }

        protected void HeavyBruiseWound(string position, int entity, params DamageTypes?[] crits)
        {
            CreateWound($"Heavy bruise on {position}", entity, DamageMultiplier * 15f,
                -1f, PainMultiplier * 45f, 0.1f, crits);
        }

        protected void WindedFromImpact(int entity)
        {
            CreateWound("Winded from impact", entity, DamageMultiplier * 20f, -1, PainMultiplier * 40f);
        }
    }
}