using GunshotWound2.Components.HitComponents.WeaponHitComponents;
using GunshotWound2.Components.WoundComponents;
using Leopotam.Ecs;

namespace GunshotWound2.Systems.DamageSystems
{
    [EcsInject]
    public abstract class BaseImpactDamageSystem<T> : BaseDamageSystem<T>
        where T : BaseWeaponHitComponent, new()
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
                -1f, PainMultiplier * 45f, -1, crits);
        }

        protected void WindedFromImpact(int entity)
        {
            CreateWound("Winded from impact", entity, DamageMultiplier * 20f, -1, PainMultiplier * 40f);
        }
    }
}