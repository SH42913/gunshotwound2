using GunshotWound2.HitDetection;
using GunshotWound2.HitDetection.WeaponHitSystems;
using Leopotam.Ecs;

namespace GunshotWound2.Damage
{
    [EcsInject]
    public abstract class BaseImpactDamageSystem<T> : BaseDamageSystem<T>
        where T : BaseWeaponHitEvent, new()
    {
        protected void DefaultGrazeWound(int entity)
        {
            CreateWound(Locale.Data.LightBruise, entity, DamageMultiplier * 3f,
                -1f, PainMultiplier * 15f);
        }

        protected void AbrasionWoundOn(string position, int entity)
        {
            CreateWound($"{Locale.Data.AbrazionWoundOn} {position}", entity, DamageMultiplier * 3f,
                BleeedingMultiplier * 0.15f, PainMultiplier * 15f);
        }

        protected void LightBruiseWound(string position, int entity)
        {
            CreateWound($"{Locale.Data.LightBruiseOn} {position}", entity, DamageMultiplier * 3f,
                -1f, PainMultiplier * 15f);
        }

        protected void MediumBruiseWound(string position, int entity)
        {
            CreateWound($"{Locale.Data.MediumBruiseOn} {position}", entity, DamageMultiplier * 8f,
                -1f, PainMultiplier * 30f, 0.001f);
        }

        protected void HeavyBruiseWound(string position, int entity, params CritTypes?[] crits)
        {
            CreateWound($"{Locale.Data.HeavyBruiseOn} {position}", entity, DamageMultiplier * 10f,
                -1f, PainMultiplier * 45f, 0.05f, crits);
        }

        protected void WindedFromImpact(int entity)
        {
            CreateWound(Locale.Data.WindedFromImpact, entity, DamageMultiplier * 10f, -1, PainMultiplier * 40f);
        }
    }
}